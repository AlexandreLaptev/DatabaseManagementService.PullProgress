using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hangfire;
using Hangfire.Storage;

namespace DatabaseManagement.Controllers
{
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IBackgroundJobClient backgroundJobs, ILogger<HomeController> logger)
        {
            _backgroundJobs = backgroundJobs;
            _logger = logger;
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

            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var queues = monitoringApi.Queues();
            if (queues.Count == 0)
            {
                _backgroundJobs.Enqueue<DatabaseUpgrater>(method => method.PerformAsync(requestId));
                _logger.LogInformation("Background job has been places in the queue. Request ID :{@requestId}", requestId);
            }

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

        [HttpGet("Error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}