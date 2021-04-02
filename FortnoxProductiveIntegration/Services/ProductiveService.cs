using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Fortnox.SDK.Entities;
using Fortnox.SDK.Exceptions;
using FortnoxProductiveIntegration.Connectors;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services
{
    public class ProductiveService : IProductiveService
    {
        private readonly HttpClient _httpClient;
        
        public ProductiveService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.productive.io/api/v2/"),
                DefaultRequestHeaders = 
                { 
                    Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") }
                }
            };
        }
        public async Task<JObject> GetUnpaidInvoiceData()
        {
            var invoiceUrl = "invoices?filter[status]=2";
            var httpMethod = HttpMethod.Get;
            var requestMessage = HttpRequestMessage(httpMethod, invoiceUrl);

            return await HttpResponseMessage(requestMessage);
        }

        public async Task UpdateInvoice(long invoiceId)
        {
            var updateUrl = $"invoice/{invoiceId}";
            var httpMethod = HttpMethod.Patch;
            var requestMessage = HttpRequestMessage(httpMethod, updateUrl);
            
            
        }
        
        public async Task<JObject> GetCustomerData(string customerId)
        {
            var contactUrl = $"contact_entries/{customerId}";
            var httpMethod = HttpMethod.Get;
            var requestMessage = HttpRequestMessage(httpMethod, contactUrl);

            return await HttpResponseMessage(requestMessage);
        }

        public async Task<JObject> GetLineItemsDataFromInvoice(string invoiceId)
        {
            var lineItemsFilterUrl = $"line_items?filter[invoice_id]={invoiceId}";
            var httpMethod = HttpMethod.Get;
            var requestMessage = HttpRequestMessage(httpMethod, lineItemsFilterUrl);

            return await HttpResponseMessage(requestMessage);
        }

        public async Task<JArray> NewInvoices(JToken dailyInvoices)
        {
            var invoiceConnector = FortnoxConnector.Invoice();
            JArray newInvoices = new JArray();
            foreach (var invoice in dailyInvoices)
            {
                Invoice exist = null;
                try
                {
                    var invoiceNumber =  (long)invoice["attributes"]?["number"];
                    exist = await invoiceConnector.GetAsync(invoiceNumber);
                }
                catch (FortnoxApiException e)
                {
                    Console.WriteLine($"{e.StatusCode}");
                }

                if (exist == null)
                {
                    newInvoices.Add(invoice);
                }
            }

            return newInvoices;
        }
        
        public JArray DailyInvoicesFilter(JToken invoicesData)
        {
            var currentDay = CurrentDay();
            JArray dailyInvoices = new JArray();
            foreach (var item in invoicesData)
            {
                var dayFromInvoice = GetCurrentDaySubstring(item);
                if (currentDay == dayFromInvoice)
                {
                    dailyInvoices.Add(item);
                }
            }

            return dailyInvoices;
        }
        
        private static string CurrentDay()
        {
            var currentDayInt = DateTime.Now.Day;
            var currentDay = $"{currentDayInt:00}";
            return currentDay;
        }
        
        private static string GetCurrentDaySubstring(JToken invoice)
        {
            var createdAt = (string)invoice["attributes"]?["created_at"];
            var substring = createdAt.Substring(0, createdAt.LastIndexOf("/", StringComparison.Ordinal));
            var currentDayString = substring.Substring(substring.IndexOf("/", StringComparison.Ordinal) + 1);
            return currentDayString;
        }
        
        private async Task<JObject> HttpResponseMessage(HttpRequestMessage requestMessage)
        {
            var responseMessage = await _httpClient.SendAsync(requestMessage);
            var jsonString = await responseMessage.Content.ReadAsStringAsync();
            var jsonObj = JObject.Parse(jsonString);

            return jsonObj;
        }

        private static HttpRequestMessage HttpRequestMessage(HttpMethod httpMethod, string path)
        {
            var requestMessage = new HttpRequestMessage(httpMethod, path)
            {
                Content = new StringContent(string.Empty, Encoding.UTF8, "application/vnd.api+json"),
                Headers =
                {
                    {"X-Auth-Token", "52ac03fa-b7e6-4d34-98d8-72676ebaafa1"},
                    {"X-Organization-Id", "14923"}
                }
            };
            return requestMessage;
        }
    }
}