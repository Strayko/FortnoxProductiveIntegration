using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Fortnox.SDK.Connectors;
using Fortnox.SDK.Entities;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services
{
    public class FortnoxService : IFortnoxService
    {
        private readonly IProductiveService _productiveService;

        public FortnoxService(IProductiveService productiveService)
        {
            _productiveService = productiveService;
        }

        public async Task<long?> CreateInvoice(JToken invoiceJObject)
        {
            var createdAt = invoiceJObject["attributes"]?["created_at"];
            var createdAtToString = Convert.ToString(createdAt);
            var createdAtToDateTime = Convert.ToDateTime(createdAtToString);
            
            var customerId = (string) invoiceJObject["relationships"]?["bill_to"]?["data"]?["id"];
            var getCustomer = await _productiveService.GetCustomerData(customerId);
            
            var invoiceId = (string) invoiceJObject["id"];
            var getLineItems = await _productiveService.GetLineItemsDataFromInvoice(invoiceId);
            
            var contact = getCustomer["data"]?["attributes"];
            var lineItems = getLineItems["data"];

            Console.WriteLine(invoiceJObject);
            
            var customerConnector = new CustomerConnector()
            {
                AccessToken = "c58e5d93-432f-4d7b-a677-f6c1e23621c3",
                ClientSecret = "WTGWLoVtqW"
            };

            var invoiceConnector = new InvoiceConnector()
            {
                AccessToken = "c58e5d93-432f-4d7b-a677-f6c1e23621c3",
                ClientSecret = "WTGWLoVtqW"
            };

            // var invoiceSearch = new InvoiceSearch()
            // {
            //     CustomerNumber = "9"
            // };

            // var invoices = await invoiceConnector.FindAsync(invoiceSearch);
            // Console.WriteLine(invoices);


            var customer = new Customer()
            {
                CustomerNumber = customerId,
                Name = (string)contact?["name"],
                Email = (string)contact?["email"],
                Phone1 = (string)contact?["phone"],
                Address1 = (string)contact?["address"],
                City = (string)contact?["city"],
                DeliveryPhone1 = (string)contact?["phone"],
                Active = true,
                Type = CustomerType.Company,
            };

            var invoiceRows = new List<InvoiceRow>();

            foreach (var item in lineItems)
            {
                invoiceRows.Add(new InvoiceRow
                {
                    Unit = (string)item["attributes"]?["unit_id"],
                    Discount = 0, 
                    Price = FormatAndParseToDecimal(item["attributes"]?["amount"]),
                    VAT = 0,
                    Description = (string)item["attributes"]?["description"], 
                    DeliveredQuantity = 1
                });
            }

            Console.WriteLine(invoiceRows);

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

            await customerConnector.CreateAsync(customer);
            var status = await invoiceConnector.CreateAsync(invoice);
            
            return status.DocumentNumber;
            
        }
        
        private static decimal FormatAndParseToDecimal(JToken price)
        {
            var decimalFormat = string.Format("{0:#.00}", Convert.ToDecimal(price) / 100);
            var decimalParse = decimal.Parse(decimalFormat);
            return decimalParse;
        }
    }
}