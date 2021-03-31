using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Fortnox.SDK;
using Fortnox.SDK.Connectors;
using Fortnox.SDK.Entities;
using Fortnox.SDK.Search;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FortnoxController : ControllerBase
    {
        private readonly IFortnoxService _fortnoxService;
        private readonly IProductiveService _productiveService;

        public FortnoxController(IFortnoxService fortnoxService, IProductiveService productiveService)
        {
            _fortnoxService = fortnoxService;
            _productiveService = productiveService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var invoicesData = await _productiveService.GetInvoiceData();
            var dailyInvoices = _productiveService.DailyInvoicesFilter(invoicesData["data"]);
          
            foreach (var invoice in dailyInvoices)
            {
                await _fortnoxService.CreateInvoice(invoice);
            }

            return Ok(new {success = "Customers and Invoices created successfully"});
        }
    }
}