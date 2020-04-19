using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using JetBrains.Annotations;

namespace DatabaseManagement
{
    public static class HangfireDbContextServiceCollectionExtensions
    {
        public static IServiceCollection InitializeHangfireDatabase(this IServiceCollection services, [NotNull] string connectionString, ILogger<Startup> logger)
        {
            logger.LogInformation("Creating Hangfire database...");

            using (var dbContext = new HangfireDbContext(connectionString))
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
            }

            logger.LogInformation("Hangfire database created.");
            return services;
        }
    }
}