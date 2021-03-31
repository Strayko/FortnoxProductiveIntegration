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

            var invoicesWithCurrentDay = invoicesRelationshipData["data"];
            var currentDayInt = DateTime.Now.Day;
            var currentDayString = Convert.ToString(currentDayInt);
            
            
            foreach (var item in invoicesWithCurrentDay)
            {
                var day = GetCurrentDaySubstring(item);
                Console.WriteLine(day);
            }

            
            
            Console.WriteLine(invoicesRelationshipData);
            var invoices = invoicesRelationshipData["data"];
           
            foreach (var invoice in invoices)
            {
                await _fortnoxService.CreateInvoice(invoice);
            }

            return Ok(new {success = "Customers and Invoices created successfully"});
        }

        private static string GetCurrentDaySubstring(JToken invoice)
        {
            var createdAt = (string)invoice["attributes"]?["created_at"];
            var substring = createdAt.Substring(0, createdAt.LastIndexOf("/", StringComparison.Ordinal));
            var currentDayString = substring.Substring(substring.IndexOf("/", StringComparison.Ordinal) + 1);
            return currentDayString;
        }
    }
}