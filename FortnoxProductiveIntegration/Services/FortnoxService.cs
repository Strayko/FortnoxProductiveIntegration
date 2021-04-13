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
using FortnoxProductiveIntegration.JsonFormat;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
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
        private readonly HttpClient _httpClient;

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
            var companyId = ConvertIdJTokenToString(invoiceJObject);
            
            var customerConnector = _connector.FortnoxCustomer();
            var invoiceConnector = _connector.FortnoxInvoice();
            
            var productiveCompany = await _productiveService.GetCompanyData(companyId);

            var fortnoxCustomer = await FortnoxCustomerExists(customerConnector, companyId);

            var customer = fortnoxCustomer ?? _mappingService.CreateFortnoxCustomer(productiveCompany);
            
            var productiveLineItem = await GetLineItems(invoiceJObject["id"]);

            var invoiceRows = productiveLineItem.Select(item => _mappingService.CreateFortnoxInvoiceRow(item)).ToList();

            var createdAt = ConvertStringToDateTimeType(invoiceJObject["attributes"]?["created_at"]);
            var dueDate = ConvertStringToDateTimeType(invoiceJObject["attributes"]?["pay_on"]);
            var deliveryDate = ConvertStringToDateTimeType(invoiceJObject["attributes"]?["delivery_on"]);
            
            var invoice = new Invoice()
            {
                Currency = (string)invoiceJObject["attributes"]?["currency"],
                ExternalInvoiceReference1 = (string)invoiceJObject["attributes"]?["number"],
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

            if (fortnoxCustomer == null)
                await customerConnector.CreateAsync(customer);

            var status = await invoiceConnector.CreateAsync(invoice);

            _logger.LogInformation($"(Fortnox) Customer with name: ({customer.Name}) and id: ({customer.CustomerNumber}) preparing and invoice");
            _logger.LogInformation($"(Fortnox) The invoice under id: ({status.DocumentNumber}) is stored");

            return status.DocumentNumber;
        }
        
        private async Task<JToken> GetFortnoxInvoice(JToken invoice)
        {
            var invoiceIdNumber = (string) invoice["attributes"]?["number"];
            var fullyPaid = await FullyPaid();

            foreach (var item in fullyPaid)
            {
                
            }

            // var invoiceIdNumber = (long) invoice["attributes"]?["number"];
            // var fortnoxInvoiceConnector = _connector.FortnoxInvoice();
            //
            // Invoice fortnoxInvoice = null;
            // try
            // {
            //     fortnoxInvoice = fortnoxInvoiceConnector.Get(invoiceIdNumber);
            // }
            // catch (Exception)
            // {
            //     // ignored
            // }
            //
            // _logger.LogInformation(fortnoxInvoice == null
            //     ? $"(Fortnox) Invoice with id: ({invoiceIdNumber}) not exists"
            //     : $"(Fortnox) Get invoice with id: ({fortnoxInvoice.DocumentNumber}) and customer name: ({fortnoxInvoice.CustomerName}) )");
            //
            // return fortnoxInvoice;

            return fullyPaid;
        }
        
        public async Task<int> CheckPaidInvoices(JToken productiveInvoices)
        {
            var paidInvoices = 0;
            foreach (var invoice in productiveInvoices)
            {
                var fortnoxInvoice = GetFortnoxInvoice(invoice);

                if (fortnoxInvoice?.FinalPayDate == null) continue;
                
                var date = fortnoxInvoice.FinalPayDate.Value.ToString("yyy-MM-dd");
                var invoiceIdFromSystem = (string) invoice["id"];
                var amount = (string) invoice["attributes"]?["amount"];

                var contentSentOn = JsonData.ContentSentOn(date);
                var contentPayments = JsonData.ContentPayments(amount, date, invoiceIdFromSystem);

                await _productiveService.SentOn(invoiceIdFromSystem, contentSentOn);
                await _productiveService.Payments(contentPayments);
                paidInvoices++;
            }

            return paidInvoices;
        }

        private async Task<JToken> FullyPaid()
        {
            var requestMessage = HttpRequestMessage();
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

        private static async Task<Customer> FortnoxCustomerExists(CustomerConnector customerConnector, string customerId)
        {
            Customer fortnoxCustomer = null;
            try
            {
                fortnoxCustomer = await customerConnector.GetAsync(customerId);
            }
            catch (Exception)
            {
                // ignored
            }

            _logger.LogInformation($"(Fortnox) User exists with id: ({customerId}) ? User: ({fortnoxCustomer})");
            return fortnoxCustomer;
        }

        private static string ConvertIdJTokenToString(JToken invoiceJObject)
        {
            var customerId = (string) invoiceJObject["relationships"]?["company"]?["data"]?["id"];
            return customerId;
        }
        
        private async Task<JObject> HttpResponseMessage(HttpRequestMessage requestMessage)
        {
            var responseMessage = await _httpClient.SendAsync(requestMessage);
            var jsonString = await responseMessage.Content.ReadAsStringAsync();
            var jsonObj = JObject.Parse(jsonString);
        
            return jsonObj;
        }
        
        private static HttpRequestMessage HttpRequestMessage()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "invoices/?filter=fullypaid")
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

        // var invoiceSearch = new InvoiceSearch()
        // {
        //     CustomerNumber = "9"
        // };

        // var invoices = await invoiceConnector.FindAsync(invoiceSearch);
        // Console.WriteLine(invoices);
    }
}