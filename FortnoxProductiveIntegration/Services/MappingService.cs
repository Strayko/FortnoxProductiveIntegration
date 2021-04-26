using System;
using Fortnox.SDK.Connectors;
using Fortnox.SDK.Entities;
using FortnoxProductiveIntegration.Services.Interfaces;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services
{
    public class MappingService : IMappingService
    {
        public Customer CreateFortnoxCustomer(JObject companyJObject, CustomerConnector customerConnector)
        {
            var customer = new Customer
            {
                Name = (string)companyJObject["data"]?["attributes"]?["name"],
                Email = (string)companyJObject["data"]?["attributes"]?["contact"]?["emails"]?[0]?["email"],
                Phone1 = (string)companyJObject["data"]?["attributes"]?["contact"]?["phones"]?[0]?["phone"],
                Address1 = (string)companyJObject["data"]?["attributes"]?["contact"]?["addresses"]?[0]?["address"],
                Address2 = (string)companyJObject["data"]?["id"],
                City = (string)companyJObject["data"]?["attributes"]?["contact"]?["addresses"]?[0]?["city"],
                DeliveryPhone1 = (string)companyJObject["data"]?["attributes"]?["contact"]?["phones"]?[0]?["phone"],
                OrganisationNumber = (string)companyJObject["data"]?["attributes"]?["vat"],
                Active = true,
                Type = CustomerType.Company
            };
            
            var newCustomer = customerConnector.CreateAsync(customer).GetAwaiter().GetResult();

            return newCustomer;
        }

        public InvoiceRow CreateFortnoxInvoiceRow(JToken item, JToken taxValue)
        {
            var quantity = (decimal)item["attributes"]?["quantity"];
            var deliveredQuantity = Math.Round(quantity);
            var vat = Math.Round((decimal) taxValue);

            var lineItem = new InvoiceRow
            {
                Unit = (string)item["attributes"]?["unit_id"],
                Discount = (decimal?) (item["attributes"]?["discount"] ?? 0),
                Price = FormatAndParseToDecimal(item["attributes"]?["amount"]),
                VAT = vat,
                Description = (string)item["attributes"]?["description"],
                DeliveredQuantity = deliveredQuantity
            };
            return lineItem;
        }
        
        private static decimal FormatAndParseToDecimal(JToken price)
        {
            var decimalFormat = string.Format("{0:#.00}", Convert.ToDecimal(price) / 100);
            var decimalParse = decimal.Parse(decimalFormat);
            return decimalParse;
        }
    }
}