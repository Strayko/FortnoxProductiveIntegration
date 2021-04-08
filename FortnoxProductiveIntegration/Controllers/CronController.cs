using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FortnoxProductiveIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CronController : ControllerBase
    {
        public async Task Get()
        {
            Console.WriteLine("test");
        }
    }
}