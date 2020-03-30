using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Hangfire;
using System.Runtime.InteropServices;

namespace DatabaseManagement.Controllers
{
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;
        private readonly CancellationToken _cancellationToken;

        public HomeController(IBackgroundJobClient backgroundJobs, IConfiguration configuration, IHostApplicationLifetime applicationLifetime, ILogger<HomeController> logger)
        {
            _backgroundJobs = backgroundJobs;
            _configuration = configuration;
            _logger = logger;
            _cancellationToken = applicationLifetime.ApplicationStopping;
        }

        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("UpdateDatabase")]
        public IActionResult UpdateDatabase()
        {
            Guid requestId = Guid.NewGuid();
            ProgressTracker.add(requestId, "Starting updating database...");

            _backgroundJobs.Enqueue(() => PerformBackgroundJob(requestId, _cancellationToken));

            return RedirectToAction("TaskProgress", new { requestId = requestId.ToString() });
        }

        [HttpGet("TaskProgress/{requestId?}")]
        public IActionResult TaskProgress(string requestId)
        {
            if (!string.IsNullOrWhiteSpace(requestId))
            {
                var statusMessage = ProgressTracker.getValue(Guid.Parse(requestId)).ToString();

                // The processing  has not yet finished.
                // Add a refresh header, to refresh the page in 2 seconds.
                Response.Headers.Add("Refresh", "2");
                return View("TaskProgress", statusMessage);
            }

            return View("TaskProgress", "Error: something went wrong with process.");
        }

        [AutomaticRetry(Attempts = 0)] // If you don’t want a job to be retried, place an explicit attribute with 0 maximum retry attempts value
        public async Task PerformBackgroundJob(Guid requestId, CancellationToken token)
        {
            var backupDirectory = _configuration["BackupDirectory"];
            string backupFilePath = string.Empty;

            try
            {
                var updateRequired = DeployDbChanges.IsUpgradeRequired(_configuration);

                token.ThrowIfCancellationRequested();

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

                    token.ThrowIfCancellationRequested();

                    // Backup database
                    BackupAndRestoreDb.BackupDB(backupFilePath, _configuration, requestId, _logger);

                    token.ThrowIfCancellationRequested();

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

                if (!token.IsCancellationRequested)
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

        [HttpGet("Error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}