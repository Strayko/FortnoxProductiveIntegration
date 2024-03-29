using System.Net.Http;
using FortnoxProductiveIntegration.Connectors;
using FortnoxProductiveIntegration.Scheduler;
using FortnoxProductiveIntegration.Services;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FortnoxProductiveIntegration
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton<IProductiveService, ProductiveService>();
            services.AddSingleton<HttpClient>();
            services.AddSingleton<IFortnoxService, FortnoxService>();
            services.AddSingleton<IMappingService, MappingService>();
            services.AddSingleton<IConnector, Connector>();

            // services.AddQuartz(q =>
            // {
            //     q.UseMicrosoftDependencyInjectionScopedJobFactory();
            //     q.AddJobAndTrigger<FortnoxCreatingNewInvoices>(Configuration);
            //     q.AddJobAndTrigger<PaidProductiveInvoices>(Configuration);
            // });
            // services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            
            services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            loggerFactory.AddFile("Logs/FortnoxProductive-{Date}.txt");

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}