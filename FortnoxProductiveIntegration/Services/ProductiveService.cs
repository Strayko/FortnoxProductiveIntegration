using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
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
        public async Task<JObject> GetInvoiceData()
        {
            var invoiceUrl = "invoices";
            var requestMessage = HttpRequestMessage(invoiceUrl);

            var responseMessage = await _httpClient.SendAsync(requestMessage);
            var invoicesJsonString = await responseMessage.Content.ReadAsStringAsync();
            var invoicesJsonObj = JObject.Parse(invoicesJsonString);
            
            return invoicesJsonObj;
        }

        public async Task<JObject> GetCustomerData(string customerId)
        {
            var contactUrl = $"contact_entries/{customerId}";
            var requestMessage = HttpRequestMessage(contactUrl);
            
            var responseMessage = await _httpClient.SendAsync(requestMessage);
            var customerJsonString = await responseMessage.Content.ReadAsStringAsync();
            var customerJsonObj = JObject.Parse(customerJsonString);
            
            return customerJsonObj;
        }

        private static HttpRequestMessage HttpRequestMessage(string path)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, path)
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