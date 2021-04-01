using System;
using System.Threading.Tasks;
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
            var invoicesData = await _fortnoxService.GetInvoiceData();
            var invoices = invoicesData["Invoices"];
            Console.WriteLine(invoices);
            
            
            
            return Ok(new {success = "Invoices"});
        }
    }
}