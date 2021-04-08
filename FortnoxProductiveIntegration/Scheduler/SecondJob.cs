using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FortnoxProductiveIntegration.Scheduler
{
    public class SecondJob : IJob
    {
        private readonly ILogger<SecondJob> _logger;

        public SecondJob(ILogger<SecondJob> logger)
        {
            _logger = logger;
        }
        
        public Task Execute(IJobExecutionContext context)
        {
            var source = new CancellationTokenSource();

            Task.Run(async delegate
            {
                await Task.Delay(1000, source.Token);
                _logger.LogInformation("Second Job");
            }, source.Token);
            
            return Task.CompletedTask;
        }
    }
}