using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.SqlServer;
using Serilog;

namespace DatabaseManagement
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        ILogger<Startup> Logger { get; }

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.InitializeHangfireDatabase(Configuration, Logger);

            // The following line enables Application Insights telemetry collection
            services.AddApplicationInsightsTelemetry();

            services.AddControllers();
            services.AddRazorPages();

            // Add Hangfire services
            services.AddHangfire(config =>
            {
                var connectionString = Configuration["HangfireConnection"];

                var storageOptions = new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                };

                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
                config.UseSimpleAssemblyNameTypeSerializer();
                config.UseRecommendedSerializerSettings();
                config.UseSqlServerStorage(connectionString, storageOptions);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseSerilogRequestLogging();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                WorkerCount = 1,
                ServerCheckInterval = TimeSpan.FromMinutes(5),
                CancellationCheckInterval = TimeSpan.FromMinutes(5)
            }); ;

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                        name: "getUpdateDatabase",
                        pattern: "Home/UpdateDatabase",
                        defaults: new { controller = "Home", action = "UpdateDatabase" });

                endpoints.MapControllerRoute(
                        name: "getTaskProgressById",
                        pattern: "Home/TaskProgress/{requestId?}",
                        defaults: new { controller = "Home", action = "TaskProgress" });

                endpoints.MapRazorPages();
            });
        }
    }
}