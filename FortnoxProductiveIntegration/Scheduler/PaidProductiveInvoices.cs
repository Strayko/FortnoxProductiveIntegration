using System;
using System.Threading;
using System.Threading.Tasks;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FortnoxProductiveIntegration.Scheduler
{
    [DisallowConcurrentExecution]
    public class PaidProductiveInvoices : IJob
    {
        private readonly ILogger<PaidProductiveInvoices> _logger;
        private readonly IFortnoxService _fortnoxService;
        private readonly IProductiveService _productiveService;

        public PaidProductiveInvoices(ILogger<PaidProductiveInvoices> logger, 
            IFortnoxService fortnoxService, 
            IProductiveService productiveService)
        {
            _logger = logger;
            _fortnoxService = fortnoxService;
            _productiveService = productiveService;
        }
        
        public Task Execute(IJobExecutionContext context)
        {
            var source = new CancellationTokenSource();

            Task.Run(async delegate
            {
                await Task.Delay(10000, source.Token);
                
                _logger.LogInformation($"A job was started PaidProductiveInvoices at: ({DateTime.Now})");
                
                var unpaidProductiveInvoices = await _productiveService.GetUnpaidInvoiceData();
                var productiveInvoices = unpaidProductiveInvoices["data"];
                var paidInvoices = await _fortnoxService.CheckPaidInvoices(productiveInvoices);
            
                _logger.LogInformation($"Number of new invoices paid: ({paidInvoices})");
                
                _logger.LogInformation($"A job was ended PaidProductiveInvoices at: ({DateTime.Now})");
                
            }, source.Token);
            
            return Task.CompletedTask;
        }
    }
}