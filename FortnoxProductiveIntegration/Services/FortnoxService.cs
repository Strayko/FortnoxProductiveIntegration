using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Fortnox.SDK.Entities;
using FortnoxProductiveIntegration.Connectors;
using FortnoxProductiveIntegration.JsonFormat;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Services
{
    public class FortnoxService : IFortnoxService
    {
        private readonly IProductiveService _productiveService;
        private readonly IMappingService _mappingService;
        private static ILogger<FortnoxService> _logger;
        private static IConnector _connector;
        private static HttpClient _httpClient;

        public FortnoxService(
            IProductiveService productiveService, 
            IMappingService mappingService, 
            ILogger<FortnoxService> logger, 
            IConnector connector)
        {
            _productiveService = productiveService;
            _mappingService = mappingService;
            _logger = logger;
            _connector = connector;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.fortnox.se/3/"),
                DefaultRequestHeaders = 
                {
                    Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") }
                }
            };
        }

        public async Task<long?> CreateInvoice(JToken invoiceJObject)
        {
            var taxValue = invoiceJObject["attributes"]?["tax1_value"];
            var companyId = ConvertCompanyIdJTokenToString(invoiceJObject);
            
            var customerConnector = _connector.FortnoxCustomer();
            var invoiceConnector = _connector.FortnoxInvoice();
            
            var productiveCompany = await _productiveService.GetCompanyData(companyId);
            var fortnoxCustomer = await FortnoxCustomerExistsFilter(companyId);

            var customer = fortnoxCustomer ?? _mappingService.CreateFortnoxCustomer(productiveCompany, customerConnector);

            var productiveLineItem = await GetLineItems(invoiceJObject["id"]);
            var invoiceRows = productiveLineItem.Select(item => _mappingService.CreateFortnoxInvoiceRow(item, taxValue)).ToList();

            var createdAt = ConvertStringToDateTimeType(invoiceJObject["attributes"]?["created_at"]);
            var dueDate = ConvertStringToDateTimeType(invoiceJObject["attributes"]?["pay_on"]);
            var deliveryDate = ConvertStringToDateTimeType(invoiceJObject["attributes"]?["delivery_on"]);
            
            var invoice = new Invoice()
            {
                Currency = (string)invoiceJObject["attributes"]?["currency"],
                ExternalInvoiceReference1 = (string)invoiceJObject["attributes"]?["number"],
                YourOrderNumber = (string)invoiceJObject["attributes"]?["number"],
                CurrencyUnit = 1,
                City = customer.City,
                Language = Language.English,
                CustomerName = customer.Name,
                CustomerNumber = customer.CustomerNumber,
                PaymentWay = PaymentWay.Card,
                CurrencyRate = 1,
                DeliveryCity = customer.DeliveryCity,
                InvoiceDate = createdAt,
                DueDate = dueDate,
                DeliveryDate = deliveryDate,
                InvoiceType = InvoiceType.CashInvoice,
                InvoiceRows = new List<InvoiceRow>(invoiceRows)
            };

            var status = await invoiceConnector.CreateAsync(invoice);
            
            _logger.LogInformation($"(Fortnox) Customer with name: ({customer.Name}) and id: ({customer.CustomerNumber}) preparing and invoice");
            _logger.LogInformation($"(Fortnox) The invoice under id: ({status.DocumentNumber}) is stored");

            return status.DocumentNumber;
        }

        private static JToken FindByNumberFilter(JToken fullyPaidInvoices, JToken invoice)
        {
            foreach (var item in fullyPaidInvoices["Invoices"])
            {
                var fortnoxNumber = (string)item["ExternalInvoiceReference1"];
                var productiveNumber = (string)invoice["attributes"]?["number"];
                
                if (fortnoxNumber == productiveNumber)
                    return item;
            }
            
            return null;
        }
        
        public async Task<int> CheckPaidInvoices(JToken productiveInvoices)
        {
            var newPaidInvoices = 0;
            var fullyPaidInvoices = await FullyPaidInvoices();

            foreach (var invoice in productiveInvoices)
            {
                var invoiceByNumber = FindByNumberFilter(fullyPaidInvoices, invoice);

                if (invoiceByNumber?["FinalPayDate"] == null) continue;
                
                var date = (string)invoiceByNumber["FinalPayDate"];
                var invoiceIdFromSystem = (string) invoice["id"];
                var amount = (string) invoice["attributes"]?["amount"];
                
                var contentSentOn = JsonData.ContentSentOn(date);
                var contentPayments = JsonData.ContentPayments(amount, date, invoiceIdFromSystem);
                
                var sut = await _productiveService.SentOn(invoiceIdFromSystem, contentSentOn);
                await _productiveService.Payments(contentPayments);
                
                _logger.LogInformation($"(Productive) Invoice with id: ({(string)invoiceByNumber["ExternalInvoiceReference1"]}) paid on day: ({date}) with amount: ({amount})");
                
                newPaidInvoices++;
            }

            return newPaidInvoices;
        }

        private static async Task<JToken> FullyPaidInvoices()
        {
            var path = "invoices/?filter=fullypaid";
            var requestMessage = HttpRequestMessage(path);
            var responseMessage = await HttpResponseMessage(requestMessage);

            return responseMessage;
        }

        private static async Task<JToken> GetAllCustomers()
        {
            var path = "customers";
            var requestMessage = HttpRequestMessage(path);
            var responseMessage = await HttpResponseMessage(requestMessage);

            return responseMessage;
        }
        
        private async Task<JToken> GetLineItems(JToken invoiceIdJToken)
        {
            var invoiceId = (string)invoiceIdJToken;
            var getLineItems = await _productiveService.GetLineItemsDataFromInvoice(invoiceId);
            var lineItems = getLineItems["data"];
            return lineItems;
        }

        private static DateTime? ConvertStringToDateTimeType(JToken dateTime)
        {
            var createdAtToString = Convert.ToString(dateTime);
            if (createdAtToString == "") return null;
            var createdAtToDateTime = Convert.ToDateTime(createdAtToString);
            return createdAtToDateTime;
        }

        private static async Task<Customer> FortnoxCustomerExistsFilter(string companyId)
        {
            var customers = await GetAllCustomers();
            var numberOfCustomers = customers["Customers"].Children().Count();
            
            _logger.LogInformation($"(Fortnox) Number of all customers from service: ({numberOfCustomers})");
            
            foreach (var customer in customers["Customers"])
            {
                if ((string) customer["OrganisationNumber"] != companyId) continue;

                var customerConnector = _connector.FortnoxCustomer();
                var customerResponse =  await customerConnector.GetAsync((string) customer["CustomerNumber"]);
                
                _logger.LogInformation($"(Fortnox) User exists with id: ({customerResponse.CustomerNumber}) ? User: ({customerResponse.Name})");
                return customerResponse;
            }
            
            _logger.LogInformation($"(Fortnox) User does not exist with id: ({companyId})");
            return null;
        }

        private static string ConvertCompanyIdJTokenToString(JToken invoiceJObject)
        {
            var customerId = (string) invoiceJObject["relationships"]?["company"]?["data"]?["id"];
            return customerId;
        }
        
        private static async Task<JObject> HttpResponseMessage(HttpRequestMessage requestMessage)
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
                Content = new StringContent(string.Empty, Encoding.UTF8, "application/json"),
                Headers =
                {
                    {"Access-Token", _connector.FortnoxAccessToken()},
                    {"Client-Secret", _connector.FortnoxClientSecret()}
                }
            };
            return requestMessage;
        }
    }
}