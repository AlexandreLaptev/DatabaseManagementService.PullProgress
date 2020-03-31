using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseManagement
{
    public static class HangfireDbContextServiceCollectionExtensions
    {
        public static IServiceCollection InitializeHangfireDatabase(this IServiceCollection services, IConfiguration configuration, ILogger<Startup> logger)
        {
            logger.LogInformation("Creating Hangfire database...");

            var initializer = new HangfireDbContextInitializer<HangfireDbContext>();
            initializer.InitializeDatabase(new HangfireDbContext());

            logger.LogInformation("Hangfire database created.");
            return services;
        }
    }
}