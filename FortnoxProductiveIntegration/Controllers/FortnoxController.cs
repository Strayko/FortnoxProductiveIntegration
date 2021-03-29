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
            var invoicesRelationshipData = await _productiveService.GetInvoiceData();
            var invoices = invoicesRelationshipData["data"];
           
            foreach (var item in invoices)
            {
                var invoice = item["attributes"];
                var customerId = (string) item["relationships"]?["bill_to"]?["data"]?["id"];
                var getCustomerFromApi = await _productiveService.GetCustomerData(customerId);
                var customer = getCustomerFromApi["data"];

                await _fortnoxService.CreateInvoice(invoice, customer);
            }

            return Ok(new {success = "Customer and Invoice created successfully"});
        }

        
    }
}