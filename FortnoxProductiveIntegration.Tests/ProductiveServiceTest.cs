using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FortnoxProductiveIntegration.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace FortnoxProductiveIntegration.Tests
{
    [TestFixture]
    public class ProductiveServiceTest
    {
        private Mock<ILogger<ProductiveService>> _logger;
        private Mock<Connectors.IConnector> _connector;
        private Mock<HttpMessageHandler> _handlerMock;

        [SetUp]
        public void SetUp()
        {
            _connector = new Mock<Connectors.IConnector>();
            _logger = new Mock<ILogger<ProductiveService>>();
            _handlerMock = new Mock<HttpMessageHandler>();
        }
        
        [Test]
        public async Task WhenGet_UnpaidInvoices_ReturnVerifyParamsArg()
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{""data"": [{""id"": ""111222"", ""type"": ""invoices"", ""attributes"": {""number"": ""52""}}]}", Encoding.UTF8, "application/vnd.api+json")
            };

            HandlerMockSetup(response);
            
            var httpClient = new HttpClient(_handlerMock.Object);
            var productiveInvoices = new ProductiveService(_logger.Object, _connector.Object, httpClient);

            var unpaidInvoiceData = await productiveInvoices.GetUnpaidInvoicesData();

            var httpMethod = HttpMethod.Get;
            AssertNotNullAndVerifyHttp(unpaidInvoiceData, httpMethod);
        }

        [Test]
        public async Task WhenInvoke_SentOn_ReturnVerifyParamsArgs()
        {
            var invoiceId = "22345";
            var contentSentOn = @"{""data"": {""type"": ""invoices"", ""attributes"": {""sent_on"": ""2021-04-21""} }";
            
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{""data"": {""id"": ""188292"", ""type"": ""invoices"", ""attributes"": {""number"": ""2021-02""}}}", Encoding.UTF8, "application/vnd.api+json")
            };

            HandlerMockSetup(response);

            var httpClient = new HttpClient(_handlerMock.Object);
            var productiveInvoices = new ProductiveService(_logger.Object, _connector.Object, httpClient);

            var sentOn = await productiveInvoices.SentOn(invoiceId, contentSentOn);

            var httpMethod = HttpMethod.Patch;
            AssertNotNullAndVerifyHttp(sentOn, httpMethod);
        }
        
        private void HandlerMockSetup(HttpResponseMessage response)
        {
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
        }
        
        private void AssertNotNullAndVerifyHttp(JObject jObject, HttpMethod httpMethod)
        {
            Assert.NotNull(jObject);
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == httpMethod),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}