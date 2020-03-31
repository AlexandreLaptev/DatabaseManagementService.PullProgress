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

        public HomeController(IBackgroundJobClient backgroundJobs)
        {
            _backgroundJobs = backgroundJobs;
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

             _backgroundJobs.Enqueue<DatabaseUpgrater>(method => method.PerformAsync(requestId));

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