using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fortnox.SDK.Interfaces;
using FortnoxProductiveIntegration.Services;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace FortnoxProductiveIntegration.Tests
{
    [TestFixture]
    public class ProductiveServiceTest
    {
        private Mock<ILogger<ProductiveService>> _logger;
        private Mock<Connectors.IConnector> _connector;

        [SetUp]
        public void SetUp()
        {
            _connector = new Mock<Connectors.IConnector>();
            _logger = new Mock<ILogger<ProductiveService>>();
        }
        
        [Test]
        public async Task WhenGet_UnpaidInvoices_VerifySendAsyncAndParamsArg()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{""data"": [{""id"": ""111222"", ""type"": ""invoices""}]}", Encoding.UTF8, "application/vnd.api+json")
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
            
            var httpClient = new HttpClient(handlerMock.Object);
            var productiveInvoices = new ProductiveService(_logger.Object, _connector.Object, httpClient);

            var unpaidInvoiceData = await productiveInvoices.GetUnpaidInvoicesData();

            Assert.NotNull(unpaidInvoiceData);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}