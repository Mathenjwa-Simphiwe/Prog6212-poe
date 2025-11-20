using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Prog6212_POE.Models;

namespace Prog6212_POE.Controllers
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
            // Show different dashboard based on role
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Lecturer"))
                {
                    ViewBag.UserRole = "Lecturer";
                }
                else if (User.IsInRole("Coordinator"))
                {
                    ViewBag.UserRole = "Coordinator";
                }
                else if (User.IsInRole("Manager"))
                {
                    ViewBag.UserRole = "Manager";
                }
                else if (User.IsInRole("HR"))
                {
                    ViewBag.UserRole = "HR";
                }
            }

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
    }
}