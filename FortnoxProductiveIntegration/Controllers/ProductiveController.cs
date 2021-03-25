using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

namespace FortnoxProductiveIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductiveController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        
        public ProductiveController()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.productive.io/api/v2/")
            };
            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/vnd.api+json");
            _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", "52ac03fa-b7e6-4d34-98d8-72676ebaafa1");
            _httpClient.DefaultRequestHeaders.Add("X-Organization-Id", "14923");
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/xml,application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5");
        }
        
        [HttpGet]
        public void Get()
        {
            
        }
    }
}