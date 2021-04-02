using System;
using System.Threading.Tasks;
using FortnoxProductiveIntegration.Connectors;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductiveController : ControllerBase
    {
        private readonly IProductiveService _productiveService;
        private readonly IFortnoxService _fortnoxService;

        public ProductiveController(IProductiveService productiveService, IFortnoxService fortnoxService)
        {
            _productiveService = productiveService;
            _fortnoxService = fortnoxService;
        }
        
        [HttpGet]
        [Route("invoices")]
        public async Task<IActionResult> Invoices()
        {
            var unpaidProductiveInvoices = await _productiveService.GetUnpaidInvoiceData();
            Console.WriteLine(unpaidProductiveInvoices);
            var productiveInvoices = unpaidProductiveInvoices["data"];

            foreach (var invoice in productiveInvoices)
            {
                var invoiceId = (long)invoice["attributes"]?["number"];

                var fortnoxInvoiceConnector = FortnoxConnector.Invoice();
                var fortnoxInvoice = fortnoxInvoiceConnector.Get(35);

                if (fortnoxInvoice.FinalPayDate != null)
                {
                    
                }
                
                
            }

            Console.WriteLine(productiveInvoices);
            
            
            
            return Ok(new {success = "Invoices"});
        }
    }
}