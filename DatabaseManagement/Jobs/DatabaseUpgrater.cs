using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Hangfire;

namespace DatabaseManagement
{
    public class DatabaseUpgrater
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseUpgrater> _logger;
        private readonly CancellationToken _cancellationToken;

        public DatabaseUpgrater(IConfiguration configuration, IHostApplicationLifetime applicationLifetime, ILogger<DatabaseUpgrater> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _cancellationToken = applicationLifetime.ApplicationStopping;
        }

        [AutomaticRetry(Attempts = 0)] // If you don’t want a job to be retried, place an explicit attribute with 0 maximum retry attempts value
        public async Task PerformAsync(Guid requestId)
        {
            var backupDirectory = _configuration["BackupDirectory"];
            string backupFilePath = string.Empty;

            try
            {
                var updateRequired = DeployDbChanges.IsUpgradeRequired(_configuration);

                _cancellationToken.ThrowIfCancellationRequested();

                if (updateRequired)
                {
                    if (!Directory.Exists(backupDirectory))
                    {
                        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            backupDirectory = backupDirectory.Replace("\\", "//");

                        Directory.CreateDirectory(backupDirectory);
                    }

                    var backupFileName = $"Northwind_{ DateTime.Now:yyyy-MM-dd-HH-mm-ss}.bak";
                    backupFilePath = Path.Combine(backupDirectory, backupFileName);
                    backupFilePath = Path.GetFullPath(backupFilePath);

                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        backupFilePath = backupFilePath.Replace("\\", "//");

                    _cancellationToken.ThrowIfCancellationRequested();

                    // Backup database
                    BackupAndRestoreDb.BackupDB(backupFilePath, _configuration, requestId, _logger);

                    _cancellationToken.ThrowIfCancellationRequested();

                    // Check if backup file created
                    if (!System.IO.File.Exists(backupFilePath))
                        throw new Exception($"Backup file '{backupFilePath}' has not been created.");

                    // Perform database upgrade
                    DeployDbChanges.PerformUpgrade(_configuration, requestId, _logger);

                    // Message of completion
                    _logger.LogInformation("All background jobs are complete.");
                    ProgressTracker.add(requestId, "Complete"); // DO NOT CHANGE value "Complete". It uses by TaskProgress.cshtml
                }
                else
                {
                    _logger.LogInformation("Upgrade is not required.");
                    ProgressTracker.add(requestId, "NotRequired"); // DO NOT CHANGE value "NotRequired". It uses by TaskProgress.cshtml
                }
            }
            catch (Exception ex)
            {
                ProgressTracker.add(requestId, $"Error: {ex.Message}"); // DO NOT CHANGE value "Error". It uses by TaskProgress.cshtml
                _logger.LogError(ex.Message);

                if (!_cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(backupFilePath) && System.IO.File.Exists(backupFilePath))
                        {
                            // Restore databse
                            BackupAndRestoreDb.RestoreDB(backupFilePath, _configuration, requestId, _logger);
                        }
                    }
                    catch (Exception e)
                    {
                        ProgressTracker.add(requestId, $"Error: {e.Message}"); // DO NOT CHANGE value "Error". It uses by TaskProgress.cshtml
                        _logger.LogError(e.Message);
                    }
                }
            }

            // Remove the backup files from the hard disk
            if (!string.IsNullOrEmpty(backupFilePath) && System.IO.File.Exists(backupFilePath))
                System.IO.File.Delete(backupFilePath);

            await Task.CompletedTask;
        }
    }
}