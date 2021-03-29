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
        public async Task<long?> CreateInvoice(JToken invoiceJObject, JToken customerJObject)
        {
            var customerId = (string)customerJObject["id"];
            var customerAttributes = customerJObject["attributes"];
            
            Console.WriteLine(invoiceJObject);
            Console.WriteLine(customerJObject);
            var customerConnector = new CustomerConnector()
            {
                AccessToken = "500ec245-710a-43a1-842a-0531c07d5754",
                ClientSecret = "WTGWLoVtqW"
            };

            var invoiceConnector = new InvoiceConnector()
            {
                AccessToken = "500ec245-710a-43a1-842a-0531c07d5754",
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
                Name = (string)customerAttributes["name"],
                Email = (string)customerAttributes["email"],
                Phone1 = (string)customerAttributes["phone"],
                Address1 = (string)customerAttributes["address"],
                City = (string)customerAttributes["city"],
                DeliveryPhone1 = (string)customerAttributes["phone"],
                Active = true,
                Type = CustomerType.Company,
            };

            var invoiceRows = new List<InvoiceRow>()
            {
                new InvoiceRow {Discount = 1, Price = 22, VAT = 0, Unit = "1", Description = "asd", DeliveredQuantity = 1}
            };

            var invoice = new Invoice()
            {
                DocumentNumber = (int)invoiceJObject["number"],
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
                InvoiceDate = DateTime.Parse((string)invoiceJObject["created_at"]),
                InvoiceType = InvoiceType.CashInvoice,
                InvoiceRows = new List<InvoiceRow>(invoiceRows)
            };

            // await customerConnector.CreateAsync(customer);
            var status = await invoiceConnector.CreateAsync(invoice);
            
            return status.DocumentNumber;
            
        }
    }
}