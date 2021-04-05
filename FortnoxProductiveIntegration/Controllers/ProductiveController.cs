using System;
using System.Threading.Tasks;
using FortnoxProductiveIntegration.Connectors;
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



            foreach (var invoice in productiveInvoices)
            {
                var invoiceIdNumber = (long)invoice["attributes"]?["number"];
                var fortnoxInvoiceConnector = FortnoxConnector.Invoice();
                var fortnoxInvoice = fortnoxInvoiceConnector.Get(invoiceIdNumber);
                
                if (fortnoxInvoice.FinalPayDate != null)
                {
                    var date = fortnoxInvoice.FinalPayDate.Value.ToString("yyy-MM-dd");
                    var invoiceIdFromSystem = (string)invoice["id"];
                    var amount = (string)invoice["attributes"]?["amount"];

                    var contentSentOn = $@"
                        {{
                          'data': {{
                            'type': 'invoices',
                            'attributes': {{
                                'sent_on': '{date}'
                            }}
                        }}
                    }}".Replace('\'', '"');

                    var contentPayments = $@"
                        {{
                           'data': {{
                              'type': 'payments',
                              'attributes': {{
                                'amount': {amount},
                                'paid_on': '{date}'
                              }},
                              'relationships': {{
                                'invoice': {{
                                  'type': 'invoices',
                                  'id': '{invoiceIdFromSystem}'
                                }}
                              }}
                            }}
                    }}".Replace('\'', '"');
                    
                    await _productiveService.SentOn(invoiceIdFromSystem, contentSentOn);

                }
            }
            
            
            return Ok(new {success = "Invoices"});
        }
    }
}