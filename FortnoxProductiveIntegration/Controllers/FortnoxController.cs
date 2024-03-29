﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FortnoxProductiveIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FortnoxController : ControllerBase
    {
        private readonly IFortnoxService _fortnoxService;
        private readonly IProductiveService _productiveService;
        private readonly ILogger<FortnoxController> _logger;

        public FortnoxController(
            IFortnoxService fortnoxService,
            IProductiveService productiveService,
            ILogger<FortnoxController> logger)
        {
            _fortnoxService = fortnoxService;
            _productiveService = productiveService;
            _logger = logger;
        }

        [HttpGet]
        [Route("invoices/create")]
        public async Task CreateInvoice()
        {
            var invoicesData = await _productiveService.GetUnpaidInvoicesData();
            var dailyInvoices = _productiveService.DailyInvoicesFilter(invoicesData["data"]);
            var newInvoices = await _productiveService.NewInvoices(dailyInvoices);
            
            if (newInvoices.Count > 0)
            {
                var newCreatedInvoices = new List<long?>();
                foreach (var invoice in newInvoices)
                {
                    var documentNumber = await _fortnoxService.CreateInvoice(invoice);

                    if (documentNumber != null)
                        newCreatedInvoices.Add(documentNumber);
                }

                _logger.LogInformation($"Number of new invoices created: ({newCreatedInvoices.Count})");
            }
            else
            {
                _logger.LogInformation($"No new invoices created");
            }
        }
    }
}