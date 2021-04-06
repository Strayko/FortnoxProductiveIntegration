using System;
using System.Threading.Tasks;
using Fortnox.SDK.Entities;
using FortnoxProductiveIntegration.Connectors;
using FortnoxProductiveIntegration.JsonFormat;
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
            var productiveInvoices = unpaidProductiveInvoices["data"];
            
            await _fortnoxService.CheckPaidInvoices(productiveInvoices);
            
            return Ok(new {success = "Successfully paid invoices!"});
        }
    }
}