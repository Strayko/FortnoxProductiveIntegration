using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FortnoxProductiveIntegration.Scheduler
{
    public class PaidProductiveInvoices : IJob
    {
        private readonly ILogger<PaidProductiveInvoices> _logger;

        public PaidProductiveInvoices(ILogger<PaidProductiveInvoices> logger)
        {
            _logger = logger;
        }
        
        public Task Execute(IJobExecutionContext context)
        {
            var source = new CancellationTokenSource();

            Task.Run(async delegate
            {
                await Task.Delay(9000, source.Token);
                
                _logger.LogInformation("A job was started PaidProductiveInvoices");
            }, source.Token);
            
            return Task.CompletedTask;
        }
    }
}