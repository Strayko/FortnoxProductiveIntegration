using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fortnox.SDK;
using Fortnox.SDK.Connectors;
using Fortnox.SDK.Entities;
using Fortnox.SDK.Exceptions;
using Fortnox.SDK.Search;
using FortnoxProductiveIntegration.Connectors;
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
        [Route("invoices/create")]
        public async Task<IActionResult> CreateInvoice()
        {
            var invoicesData = await _productiveService.GetUnpaidInvoiceData();
            var dailyInvoices = _productiveService.DailyInvoicesFilter(invoicesData["data"]);
            var newInvoices = await _productiveService.NewInvoices(dailyInvoices);
            
            if (newInvoices.Count > 0)
            {
                foreach (var invoice in newInvoices)
                {
                    await _fortnoxService.CreateInvoice(invoice);
                }

                return Ok(new {success = "Invoices created successfully!"});
            }

            return Ok(new {success = "No new invoices!"});
        }
    }
}