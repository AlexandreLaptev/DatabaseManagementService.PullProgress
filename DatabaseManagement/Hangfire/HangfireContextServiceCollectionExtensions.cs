using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseManagement
{
    public static class HangfireContextServiceCollectionExtensions
    {
        public static IServiceCollection CreateHangfireDatabase(this IServiceCollection services, IConfiguration configuration, ILogger<Startup> logger)
        {
            logger.LogInformation("Creating Hangfire database...");

            services.AddDbContext<HangfireContext>(options =>
            {
                options.UseSqlServer(configuration["HangfireConnection"]);
            });

            // Create an instance of HangfireContext so we can use it early in the startup process.
            // By default, this service is not instantiated and available until later during the startup process.
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                // Getting HangfireContext will delete and create Hangfire database.
                // By default, LocalDB database creates “*.mdf” files in the C:/Users/<user> directory.
                using (var dbContext = scope.ServiceProvider.GetRequiredService<HangfireContext>())
                {
                };
            }

            logger.LogInformation("Hangfire database created.");
            return services;
        }
    }
}