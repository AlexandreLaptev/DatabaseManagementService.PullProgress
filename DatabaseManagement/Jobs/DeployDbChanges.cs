using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using DbUp;
using DbUp.ScriptProviders;

namespace DatabaseManagement
{
    internal class DeployDbChanges
    {
        public static void PerformUpgrade(IConfiguration configuration, Guid requestId, ILogger<DatabaseUpgrater> logger)
        {
            PerformSchemaUpgrade(configuration, requestId, logger);
            PerformDataUpgrade(configuration, requestId, logger);
        }

        public static void PerformSchemaUpgrade(IConfiguration configuration, Guid requestId, ILogger<DatabaseUpgrater> logger)
        {
            string scriptsPath = GetScriptsDirectory("Schema", configuration);

            if (Directory.GetFiles(scriptsPath, "*.sql", SearchOption.TopDirectoryOnly).Length > 0)
            {
                logger.LogInformation("Start performing schema upgrade...");
                ProgressTracker.add(requestId, "Start performing schema upgrade...");

                var connectionString = configuration["NorthwindConnection"];

                var scriptsExecutor =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsFromFileSystem
                    (
                        scriptsPath,
                        new FileSystemScriptOptions { IncludeSubDirectories = false }
                    )
                    .WithTransaction() // apply all changes in a single transaction
                    .LogToConsole();

                var upgrader = scriptsExecutor.Build();

                //Check if an upgrade is required
                if (upgrader.IsUpgradeRequired())
                {
                    var upgradeResult = upgrader.PerformUpgrade();

                    if (upgradeResult.Successful)
                    {
                        var message = "Database schema upgraded successfully.";
                        logger.LogInformation(message);
                        ProgressTracker.add(requestId, message);
                    }
                    else
                    {
                        var message = "Error performing schema upgrade.";
                        logger.LogError(message);
                        ProgressTracker.add(requestId, message);
                        throw new Exception($"{message}: ", upgradeResult.Error);
                    }
                }
                else
                {
                    var message = "Schema upgrade is not required.";
                    logger.LogInformation(message);
                    ProgressTracker.add(requestId, message);
                }
            }
        }

        public static void PerformDataUpgrade(IConfiguration configuration, Guid requestId, ILogger<DatabaseUpgrater> logger)
        {
            string scriptsPath = GetScriptsDirectory("Data", configuration);

            if (Directory.GetFiles(scriptsPath, "*.sql", SearchOption.TopDirectoryOnly).Length > 0)
            {
                logger.LogInformation("Start performing data upgrade...");
                ProgressTracker.add(requestId, "Start performing data upgrade...");

                var connectionString = configuration["NorthwindConnection"];

                var scriptsExecutor =
                    DeployChanges.To
                        .SqlDatabase(connectionString)
                        .WithScriptsFromFileSystem
                        (
                            scriptsPath,
                            new FileSystemScriptOptions { IncludeSubDirectories = false }
                        )
                        .WithTransaction() // apply all changes in a single transaction
                        .LogToConsole();

                var upgrader = scriptsExecutor.Build();

                // Check if an upgrade is required
                if (upgrader.IsUpgradeRequired())
                {
                    var upgradeResult = upgrader.PerformUpgrade();

                    if (upgradeResult.Successful)
                    {
                        var message = "Database data upgraded successfully.";
                        logger.LogInformation(message);
                        ProgressTracker.add(requestId, message);
                    }
                    else
                    {
                        var message = "Error performing data upgrade.";
                        logger.LogError(message);
                        ProgressTracker.add(requestId, message);
                        throw new Exception($"{message}: ", upgradeResult.Error);
                    }
                }
                else
                {
                    var message = "Data update is not required.";
                    logger.LogInformation(message);
                    ProgressTracker.add(requestId, message);
                }
            }
        }

        public static bool IsUpgradeRequired(IConfiguration configuration)
        {
            var updateRequired = IsSchemaUpgradeRequired(configuration);

            if (updateRequired)
                return true;

            return IsDataUpgradeRequired(configuration);
        }

        public static bool IsSchemaUpgradeRequired(IConfiguration configuration)
        {
            string scriptsPath = GetScriptsDirectory("Schema", configuration);

            if (Directory.GetFiles(scriptsPath, "*.sql", SearchOption.TopDirectoryOnly).Length > 0)
            {
                var connectionString = configuration["NorthwindConnection"];

                var scriptsExecutor =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsFromFileSystem
                    (
                        scriptsPath,
                        new FileSystemScriptOptions { IncludeSubDirectories = false }
                    )
                    .WithTransaction() // apply all changes in a single transaction
                    .LogToConsole();

                var upgrader = scriptsExecutor.Build();
                return upgrader.IsUpgradeRequired();
            }

            return false;
        }

        public static bool IsDataUpgradeRequired(IConfiguration configuration)
        {
            string scriptsPath = GetScriptsDirectory("Data", configuration);

            if (Directory.GetFiles(scriptsPath, "*.sql", SearchOption.TopDirectoryOnly).Length > 0)
            {
                var connectionString = configuration["NorthwindConnection"];

                var dataScriptsExecutor =
                    DeployChanges.To
                        .SqlDatabase(connectionString)
                        .WithScriptsFromFileSystem
                        (
                            scriptsPath,
                            new FileSystemScriptOptions { IncludeSubDirectories = false }
                        )
                        .WithTransaction() // apply all changes in a single transaction
                        .LogToConsole();

                var upgrader = dataScriptsExecutor.Build();
                return upgrader.IsUpgradeRequired();
            }

            return false;
        }

        private static string GetScriptsDirectory(string subDirName, IConfiguration configuration)
        {
            var scriptsDirectory = configuration["ScriptsDirectory"];
            var scriptsPath = Path.GetFullPath(scriptsDirectory);

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                scriptsPath = scriptsPath.Replace("\\", "//");

            if (!Directory.Exists(scriptsPath))
                throw new MissingFieldException($"Error: database scripts directoty '{scriptsPath}' not found.");

            string subDirectoryPath = Path.Combine(scriptsDirectory, subDirName);

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                subDirectoryPath = subDirectoryPath.Replace("\\", "//");

            return subDirectoryPath;
        }
    }
}