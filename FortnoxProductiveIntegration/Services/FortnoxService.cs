using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Fortnox.SDK.Connectors;
using Fortnox.SDK.Entities;
using Fortnox.SDK.Exceptions;
using FortnoxProductiveIntegration.Connectors;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services
{
    public class FortnoxService : IFortnoxService
    {
        private readonly IProductiveService _productiveService;
        private readonly IMappingService _mappingService;

        public FortnoxService(IProductiveService productiveService, IMappingService mappingService)
        {
            _productiveService = productiveService;
            _mappingService = mappingService;
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

            var createdAtToDateTime = ConvertStringToDateTimeType(invoiceJObject["attributes"]?["created_at"]);
            var invoice = new Invoice()
            {
                DocumentNumber = (long)invoiceJObject["attributes"]?["number"],
                Address1 = "address1",
                Address2 = "address2",
                Currency = "SEK",
                CurrencyUnit = 1,
                City = "city",
                Language = Language.English,
                CustomerName = customer.Name,
                CustomerNumber = customer.CustomerNumber,
                PaymentWay = PaymentWay.Card,
                CurrencyRate = 1,
                DeliveryCity = "delivery city",
                InvoiceDate = createdAtToDateTime,
                InvoiceType = InvoiceType.CashInvoice,
                InvoiceRows = new List<InvoiceRow>(invoiceRows)
            };

            if (fortnoxCustomer == null)
                await customerConnector.CreateAsync(customer);

            var status = await invoiceConnector.CreateAsync(invoice);
            
            return status.DocumentNumber;
        }
        
        private async Task<JToken> GetLineItems(JToken invoiceIdJToken)
        {
            var invoiceId = (string)invoiceIdJToken;
            var getLineItems = await _productiveService.GetLineItemsDataFromInvoice(invoiceId);
            var lineItems = getLineItems["data"];
            return lineItems;
        }

        private static DateTime ConvertStringToDateTimeType(JToken createdAt)
        {
            var createdAtToString = Convert.ToString(createdAt);
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
            catch (FortnoxApiException e)
            {
                Console.WriteLine($"{e.StatusCode}");
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