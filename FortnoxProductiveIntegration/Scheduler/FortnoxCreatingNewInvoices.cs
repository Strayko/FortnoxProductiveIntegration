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

        public FortnoxCreatingNewInvoices(ILogger<FortnoxCreatingNewInvoices> logger)
        {
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("A job was started FortnoxCreatingNewInvoices");

            return Task.CompletedTask;
        }
    }
}