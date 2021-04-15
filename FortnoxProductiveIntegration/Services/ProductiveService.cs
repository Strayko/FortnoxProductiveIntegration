using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Fortnox.SDK.Search;
using FortnoxProductiveIntegration.Connectors;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services
{
    public class ProductiveService : IProductiveService
    {
        private readonly ILogger<ProductiveService> _logger;
        private static IConnector _connector;
        private readonly HttpClient _httpClient;
        private const string EmptyContent = ""; 
        
        public ProductiveService(ILogger<ProductiveService> logger, IConnector connector)
        {
            _logger = logger;
            _connector = connector;
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
            var requestMessage = HttpRequestMessage(httpMethod, invoiceUrl, EmptyContent);

            var invoices = await HttpResponseMessage(requestMessage);
            
            _logger.LogInformation($"(Productive) Number of unpaid invoices: ({invoices["meta"]?["total_count"]})");
            return invoices;
        }

        public async Task<JObject> SentOn(string invoiceId, string contentSentOn)
        {
            var updateUrl = $"invoices/{invoiceId}";
            var httpMethod = HttpMethod.Patch;
            var requestMessage = HttpRequestMessage(httpMethod, updateUrl, contentSentOn);

            var sentOn = await HttpResponseMessage(requestMessage);
            
            _logger.LogInformation($"(Productive) Invoice with id: ({sentOn["data"]?["attributes"]?["number"]}) sent on date: ({sentOn["data"]?["attributes"]?["sent_on"]})");
            return sentOn;
        }

        public async Task<JObject> Payments(string contentPayments)
        {
            var paymentsUrl = $"payments";
            var httpMethod = HttpMethod.Post;
            var requestMessage = HttpRequestMessage(httpMethod, paymentsUrl, contentPayments);

            var payments = await HttpResponseMessage(requestMessage);
          
            _logger.LogInformation($"(Productive) And paid on: ({payments["data"]?["attributes"]?["paid_on"]})");
            return payments;
        }
        
        public async Task<JObject> GetCompanyData(string companyId)
        {
            var contactUrl = $"companies/{companyId}";
            var httpMethod = HttpMethod.Get;
            var requestMessage = HttpRequestMessage(httpMethod, contactUrl, EmptyContent);

            var company = await HttpResponseMessage(requestMessage);
            
            _logger.LogInformation($"(Productive) Get company data: ({company["data"]?["attributes"]?["name"]})");
            return company;
        }

        public async Task<JObject> GetLineItemsDataFromInvoice(string invoiceId)
        {
            var lineItemsFilterUrl = $"line_items?filter[invoice_id]={invoiceId}";
            var httpMethod = HttpMethod.Get;
            var requestMessage = HttpRequestMessage(httpMethod, lineItemsFilterUrl, EmptyContent);

            var lineItems = await HttpResponseMessage(requestMessage);
            
            _logger.LogInformation($"Current number of line items: ({lineItems["meta"]?["total_count"]})");
            return lineItems;
        }

        public async Task<JArray> NewInvoices(JToken dailyInvoices)
        {
            var invoiceConnector = _connector.FortnoxInvoice();
            JArray newInvoices = new JArray();
            foreach (var invoice in dailyInvoices)
            {
                var invoiceSearch = new InvoiceSearch
                {
                    ExternalInvoiceReference1 = (string)invoice["attributes"]?["number"]
                };

                var invoiceEntityCollection = await invoiceConnector.FindAsync(invoiceSearch);
                
                if (invoiceEntityCollection?.Entities.Count == 0)
                    newInvoices.Add(invoice);
            }

            _logger.LogInformation($"(Productive) Number of new invoices: ({newInvoices.Count})");
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

        private static HttpRequestMessage HttpRequestMessage(HttpMethod httpMethod, string path, string content)
        {
            var requestMessage = new HttpRequestMessage(httpMethod, path)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/vnd.api+json"),
                Headers =
                {
                    {"X-Auth-Token", _connector.ProductiveXAuthToken()},
                    {"X-Organization-Id", _connector.ProductiveXOrganizationId()}
                }
            };
            return requestMessage;
        }
    }
}