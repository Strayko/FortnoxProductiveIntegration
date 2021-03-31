using System;
using Fortnox.SDK.Entities;
using FortnoxProductiveIntegration.Services.Interfaces;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services
{
    public class MappingService : IMappingService
    {
        public Customer CreateFortnoxCustomer(JObject customerJObject)
        {
            var customer = new Customer
            {
                CustomerNumber = (string)customerJObject["data"]?["id"],
                Name = (string)customerJObject["data"]?["attributes"]?["name"],
                Email = (string)customerJObject["data"]?["attributes"]?["email"],
                Phone1 = (string)customerJObject["data"]?["attributes"]?["phone"],
                Address1 = (string)customerJObject["data"]?["attributes"]?["address"],
                City = (string)customerJObject["data"]?["attributes"]?["city"],
                DeliveryPhone1 = (string)customerJObject["data"]?["attributes"]?["phone"],
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
                Discount = 0, 
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