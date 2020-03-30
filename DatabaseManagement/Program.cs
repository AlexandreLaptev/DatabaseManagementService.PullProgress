using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DatabaseManagement
{
    public class Program
    {
        /// <summary>
        /// Main application endpoint.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddEnvironmentVariables("ASPNETCORE_")
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                // Uses the AddCommandLine() method that allows to specify the hosting environment at run time using the --environment argument:
                // > dotnet run --environment "Staging" or > dotnet run --environment="Production"
                .AddCommandLine(args);

            var config = builder.Build();

            var webHostBuilder = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .ConfigureLogging((context, logging) =>
                {
                    // Clear all previously registered providers
                    logging.ClearProviders();

                    // Now register everything you want
                    logging.AddConfiguration(context.Configuration.GetSection("Logging"));

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    }
                });

            var host = webHostBuilder.Build();
            host.Run();
        }
    }
}
