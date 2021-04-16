using System;
using System.Threading;
using System.Threading.Tasks;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FortnoxProductiveIntegration.Scheduler
{
    [DisallowConcurrentExecution]
    public class FortnoxCreatingNewInvoices : IJob
    {
        private readonly ILogger<FortnoxCreatingNewInvoices> _logger;
        private readonly IFortnoxService _fortnoxService;
        private readonly IProductiveService _productiveService;

        public FortnoxCreatingNewInvoices(ILogger<FortnoxCreatingNewInvoices> logger, 
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
                _logger.LogInformation($"-----(START:FORTNOX JOB) A job was started FortnoxCreatingNewInvoices at: ({DateTime.Now})-----");
            
                var invoicesData = await _productiveService.GetUnpaidInvoicesData();
                var dailyInvoices = _productiveService.DailyInvoicesFilter(invoicesData["data"]);
                var newInvoices = await _productiveService.NewInvoices(dailyInvoices);

                if (newInvoices.Count > 0)
                {
                    foreach (var invoice in newInvoices)
                    {
                        await _fortnoxService.CreateInvoice(invoice);
                    }

                    _logger.LogInformation($"Number of new invoices created: ({newInvoices.Count})");
                }
                else
                {
                    _logger.LogInformation($"No new invoices created");
                }
            
                _logger.LogInformation($"-----(END:FORTNOX JOB) A job was ended FortnoxCreatingNewInvoices at: ({DateTime.Now})-----");
            }, source.Token);
            
            return Task.CompletedTask;
        }
    }
}