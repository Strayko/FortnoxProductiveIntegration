﻿using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FortnoxProductiveIntegration.Entites;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FortnoxProductiveIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductiveController : ControllerBase
    {
        private readonly IProductiveService _productiveService;

        public ProductiveController(IProductiveService productiveService)
        {
            _productiveService = productiveService;
        }
        
        [HttpGet]
        public async Task<JObject> Get()
        {
            var invoiceData = await _productiveService.GetInvoiceData();

            var data = invoiceData["data"];
            Console.WriteLine(data);
            
            
            
            return invoiceData;
        }
    }
}