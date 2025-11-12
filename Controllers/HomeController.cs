using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ReportSystem.Models;
using System.Diagnostics;

namespace ReportSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Handle 404 Not Found errors
        public IActionResult NotFoundPage()
        {
            var feature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            if (feature != null)
            {
                ViewData["Path"] = feature.OriginalPath;
            }
            else
            {
                ViewData["Path"] = HttpContext.Request.Path;
            }

            return View("NotFound");
        }

        // Handle 403 Forbidden errors
        public IActionResult ForbiddenPage()
        {
            var originalPath = HttpContext.Request.Query["originalPath"].FirstOrDefault();
            ViewData["Path"] = originalPath;
            return View("Forbidden");
        }

    }
}
