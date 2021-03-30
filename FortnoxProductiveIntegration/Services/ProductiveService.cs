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

            return await HttpResponseAndReturnToJsonObj(requestMessage);
        }

        public async Task<JObject> GetCustomerData(string customerId)
        {
            var contactUrl = $"contact_entries/{customerId}";
            var requestMessage = HttpRequestMessage(contactUrl);

            return await HttpResponseAndReturnToJsonObj(requestMessage);
        }

        public async Task<JObject> GetLineItemsDataFromInvoice(string invoiceId)
        {
            var lineItemsFilterUrl = $"line_items?filter[invoice_id]={invoiceId}";
            var requestMessage = HttpRequestMessage(lineItemsFilterUrl);

            return await HttpResponseAndReturnToJsonObj(requestMessage);
        }
        
        private async Task<JObject> HttpResponseAndReturnToJsonObj(HttpRequestMessage requestMessage)
        {
            var responseMessage = await _httpClient.SendAsync(requestMessage);
            var jsonString = await responseMessage.Content.ReadAsStringAsync();
            var jsonObj = JObject.Parse(jsonString);

            return jsonObj;
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