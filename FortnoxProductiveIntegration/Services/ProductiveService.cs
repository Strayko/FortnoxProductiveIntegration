using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FortnoxProductiveIntegration.Services.Interfaces;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services
{
    public class ProductiveService : IProductiveServices
    {
        public async Task<JObject> GetInvoiceData()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.productive.io/api/v2/"),
                DefaultRequestHeaders = 
                { 
                    Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") }
                }
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "invoices")
            {
                Content = new StringContent(string.Empty, Encoding.UTF8, "application/vnd.api+json"),
                Headers = 
                {
                    { "X-Auth-Token", "52ac03fa-b7e6-4d34-98d8-72676ebaafa1" },
                    { "X-Organization-Id", "14923" }
                }
            };

            var responseMessage = await httpClient.SendAsync(requestMessage);
            var invoicesJsonString = await responseMessage.Content.ReadAsStringAsync();
            var invoicesJsonObj = JObject.Parse(invoicesJsonString);
            
            return invoicesJsonObj;
        }
    }
}