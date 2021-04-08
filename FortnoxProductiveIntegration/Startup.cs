using FortnoxProductiveIntegration.Scheduler;
using FortnoxProductiveIntegration.Services;
using FortnoxProductiveIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
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

            services.AddScoped<IProductiveService, ProductiveService>();
            services.AddScoped<IFortnoxService, FortnoxService>();
            services.AddScoped<IMappingService, MappingService>();

            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionScopedJobFactory();
                
                var jobKey = new JobKey("HelloWorldJob");
                q.AddJob<HelloWorldJob>(opts => opts.WithIdentity(jobKey));
                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("HelloWorldJob-trigger")
                    .WithCronSchedule("0/5 * * * * ?"));

                var jobKey2 = new JobKey("SecondJob");
                q.AddJob<SecondJob>(opts => opts.WithIdentity(jobKey2));
                q.AddTrigger(opts => opts
                    .ForJob(jobKey2)
                    .WithIdentity("SecondJob-trgger")
                    .WithCronSchedule("0/5 * * * * ?"));

            });
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            
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