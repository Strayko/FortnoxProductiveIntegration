using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
        private readonly HttpClient _httpClient;

        public FortnoxService(IProductiveService productiveService, IMappingService mappingService)
        {
            _productiveService = productiveService;
            _mappingService = mappingService;
            
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.fortnox.se/3/"),
                DefaultRequestHeaders = 
                { 
                    Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") }
                }
            };
        }

        public async Task<JObject> GetInvoiceData()
        {
            var invoiceUrl = "invoices/?filter=fullypaid";
            var requestMessage = HttpRequestMessage(invoiceUrl);

            return await HttpResponseMessage(requestMessage);
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
                Currency = (string)invoiceJObject["attributes"]?["currency"],
                CurrencyUnit = 1,
                City = customer.City,
                Language = Language.English,
                CustomerName = customer.Name,
                CustomerNumber = customer.CustomerNumber,
                PaymentWay = PaymentWay.Card,
                CurrencyRate = 1,
                DeliveryCity = customer.DeliveryCity,
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
        
        private static HttpRequestMessage HttpRequestMessage(string path)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, path)
            {
                Content = new StringContent(string.Empty, Encoding.UTF8, "application/json"),
                Headers =
                {
                    {"Access-Token", FortnoxCredentials.AccessToken},
                    {"Client-Secret", FortnoxCredentials.ClientSecret}
                }
            };
            return requestMessage;
        }
        
        private async Task<JObject> HttpResponseMessage(HttpRequestMessage requestMessage)
        {
            var responseMessage = await _httpClient.SendAsync(requestMessage);
            var jsonString = await responseMessage.Content.ReadAsStringAsync();
            var jsonObj = JObject.Parse(jsonString);

            return jsonObj;
        }

        // var invoiceSearch = new InvoiceSearch()
        // {
        //     CustomerNumber = "9"
        // };

        // var invoices = await invoiceConnector.FindAsync(invoiceSearch);
        // Console.WriteLine(invoices);
    }
}