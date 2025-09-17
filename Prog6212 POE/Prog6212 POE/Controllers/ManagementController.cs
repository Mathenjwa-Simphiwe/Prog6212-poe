using Microsoft.AspNetCore.Mvc;

namespace Prog6212_POE.Controllers
{
    public class ManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
