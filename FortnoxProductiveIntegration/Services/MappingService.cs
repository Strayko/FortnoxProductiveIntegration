using System;
using Fortnox.SDK.Entities;
using FortnoxProductiveIntegration.Services.Interfaces;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services
{
    public class MappingService : IMappingService
    {
        public Customer CreateFortnoxCustomer(JObject companyJObject)
        {
            var customer = new Customer
            {
                CustomerNumber = (string)companyJObject["data"]?["id"],
                Name = (string)companyJObject["data"]?["attributes"]?["name"],
                Email = (string)companyJObject["data"]?["attributes"]?["contact"]?["emails"]?[0]?["email"],
                Phone1 = (string)companyJObject["data"]?["attributes"]?["contact"]?["phones"]?[0]?["phone"],
                Address1 = (string)companyJObject["data"]?["attributes"]?["contact"]?["addresses"]?[0]?["address"],
                City = (string)companyJObject["data"]?["attributes"]?["contact"]?["addresses"]?[0]?["city"],
                DeliveryPhone1 = (string)companyJObject["data"]?["attributes"]?["contact"]?["phones"]?[0]?["phone"],
                Active = true,
                Type = CustomerType.Company
            };
            return customer;
        }

        public InvoiceRow CreateFortnoxInvoiceRow(JToken item)
        {
            var lineItem = new InvoiceRow
            {
                Unit = (string)item["attributes"]?["unit_id"],
                Discount = (decimal?) (item["attributes"]?["discount"] ?? 0),
                Price = FormatAndParseToDecimal(item["attributes"]?["amount"]),
                VAT = 0,
                Description = (string)item["attributes"]?["description"],
                DeliveredQuantity = 1
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