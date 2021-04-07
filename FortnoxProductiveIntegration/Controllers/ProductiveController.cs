using System.Threading.Tasks;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FortnoxProductiveIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductiveController : ControllerBase
    {
        private readonly IProductiveService _productiveService;
        private readonly IFortnoxService _fortnoxService;
        private readonly ILogger<ProductiveController> _logger;

        public ProductiveController(
            IProductiveService productiveService, 
            IFortnoxService fortnoxService, 
            ILogger<ProductiveController> logger)
        {
            _productiveService = productiveService;
            _fortnoxService = fortnoxService;
            _logger = logger;
        }
        
        [HttpGet]
        [Route("invoices")]
        public async Task Invoices()
        {
            var unpaidProductiveInvoices = await _productiveService.GetUnpaidInvoiceData();
            var productiveInvoices = unpaidProductiveInvoices["data"];
            
            var paidInvoices = await _fortnoxService.CheckPaidInvoices(productiveInvoices);
            
            _logger.LogInformation($"Number of new invoices paid: ({paidInvoices})");
        }
    }
}