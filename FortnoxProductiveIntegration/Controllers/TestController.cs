using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Fortnox.SDK;
using Fortnox.SDK.Connectors;
using Fortnox.SDK.Entities;
using Fortnox.SDK.Search;
using Microsoft.AspNetCore.Mvc;

namespace FortnoxProductiveIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
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
                CustomerNumber = "9",
                Name = "Neven Docic",
                Phone1 = "333444",
                Phone2 = "333444",
                Active = true,
                DeliveryPhone1 = "333444",
                Type = CustomerType.Company,
            };
            
            var invoiceRows = new List<InvoiceRow>()
            {
                new InvoiceRow { Discount = 1, Price = 22, VAT = 0, Unit = "1", Description = "asd", DeliveredQuantity = 1}
            };

            var invoice = new Invoice()
            {
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
                InvoiceDate = DateTime.Now,
                InvoiceType = InvoiceType.CashInvoice,
                InvoiceRows = new List<InvoiceRow>(invoiceRows)
            };

            // await customerConnector.CreateAsync(customer);
            // await invoiceConnector.CreateAsync(invoice);

            return Ok(new {success = "Customer and Invoice created successfully"});
        }
    }
}