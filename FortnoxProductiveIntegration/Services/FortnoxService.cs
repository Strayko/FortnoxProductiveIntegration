﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Fortnox.SDK.Connectors;
using Fortnox.SDK.Entities;
using Fortnox.SDK.Exceptions;
using FortnoxProductiveIntegration.Connectors;
using FortnoxProductiveIntegration.JsonFormat;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services
{
    public class FortnoxService : IFortnoxService
    {
        private readonly IProductiveService _productiveService;
        private readonly IMappingService _mappingService;
        private readonly ILogger<FortnoxService> _log;

        public FortnoxService(IProductiveService productiveService, IMappingService mappingService, ILogger<FortnoxService> log)
        {
            _productiveService = productiveService;
            _mappingService = mappingService;
            _log = log;
        }

        public async Task<long?> CreateInvoice(JToken invoiceJObject)
        {
            var customerId = ConvertIdJTokenToString(invoiceJObject);
            
            var customerConnector = FortnoxConnector.Customer();
            var invoiceConnector = FortnoxConnector.Invoice();
            
            var productiveCustomer = await _productiveService.GetCustomerData(customerId);
            var fortnoxCustomer = await FortnoxCustomerExists(customerConnector, customerId);

            var customer = fortnoxCustomer ?? _mappingService.CreateFortnoxCustomer(productiveCustomer);

            var productiveLineItem = await GetLineItems(invoiceJObject["id"]);

            var invoiceRows = productiveLineItem.Select(item => _mappingService.CreateFortnoxInvoiceRow(item)).ToList();

            var createdAt = ConvertStringToDateTimeType(invoiceJObject["attributes"]?["created_at"]);
            var dueDate = ConvertStringToDateTimeType(invoiceJObject["attributes"]?["pay_on"]);
            var deliveryDate = ConvertStringToDateTimeType(invoiceJObject["attributes"]?["delivery_on"]);
            
            var invoice = new Invoice()
            {
                DocumentNumber = (long)invoiceJObject["attributes"]?["number"],
                Currency = (string)invoiceJObject["attributes"]?["currency"],
                CurrencyUnit = 1,
                City = customer.City,
                Language = Language.English,
                CustomerName = customer.Name,
                CustomerNumber = customer.CustomerNumber,
                PaymentWay = PaymentWay.Card,
                CurrencyRate = 1,
                DeliveryCity = customer.DeliveryCity,
                InvoiceDate = createdAt,
                DueDate = dueDate,
                DeliveryDate = deliveryDate,
                InvoiceType = InvoiceType.CashInvoice,
                InvoiceRows = new List<InvoiceRow>(invoiceRows)
            };

            if (fortnoxCustomer == null)
                await customerConnector.CreateAsync(customer);

            var status = await invoiceConnector.CreateAsync(invoice);

            _log.LogInformation($"The invoice under id {status.DocumentNumber} is stored.");
            
            return status.DocumentNumber;
        }
        
        public Invoice GetFortnoxInvoice(JToken invoice)
        {
            var invoiceIdNumber = (long) invoice["attributes"]?["number"];
            var fortnoxInvoiceConnector = FortnoxConnector.Invoice();
            Invoice fortnoxInvoice = null;
            try
            {
                fortnoxInvoice = fortnoxInvoiceConnector.Get(invoiceIdNumber);
            }
            catch (Exception e)
            {
                // ignored
            }
            
            return fortnoxInvoice;
        }
        
        public async Task CheckPaidInvoices(JToken productiveInvoices)
        {
            foreach (var invoice in productiveInvoices)
            {
                var fortnoxInvoice = GetFortnoxInvoice(invoice);

                if (fortnoxInvoice.FinalPayDate != null)
                {
                    var date = fortnoxInvoice.FinalPayDate.Value.ToString("yyy-MM-dd");
                    var invoiceIdFromSystem = (string) invoice["id"];
                    var amount = (string) invoice["attributes"]?["amount"];

                    var contentSentOn = JsonData.ContentSentOn(date);
                    var contentPayments = JsonData.ContentPayments(amount, date, invoiceIdFromSystem);

                    await _productiveService.SentOn(invoiceIdFromSystem, contentSentOn);
                    await _productiveService.Payments(contentPayments);
                }
            }
        }
        
        private async Task<JToken> GetLineItems(JToken invoiceIdJToken)
        {
            var invoiceId = (string)invoiceIdJToken;
            var getLineItems = await _productiveService.GetLineItemsDataFromInvoice(invoiceId);
            var lineItems = getLineItems["data"];
            return lineItems;
        }

        private static DateTime? ConvertStringToDateTimeType(JToken dateTime)
        {
            var createdAtToString = Convert.ToString(dateTime);
            if (createdAtToString == "") return null;
            var createdAtToDateTime = Convert.ToDateTime(createdAtToString);
            return createdAtToDateTime;
        }

        private static async Task<Customer> FortnoxCustomerExists(CustomerConnector customerConnector, string customerId)
        {
            Customer fortnoxCustomer = null;
            try
            {
                fortnoxCustomer = await customerConnector.GetAsync(customerId);
            }
            catch (Exception e)
            {
                // ignored
            }

            return fortnoxCustomer;
        }

        private static string ConvertIdJTokenToString(JToken invoiceJObject)
        {
            var customerId = (string) invoiceJObject["relationships"]?["bill_to"]?["data"]?["id"];
            return customerId;
        }

        // var invoiceSearch = new InvoiceSearch()
        // {
        //     CustomerNumber = "9"
        // };

        // var invoices = await invoiceConnector.FindAsync(invoiceSearch);
        // Console.WriteLine(invoices);
    }
}