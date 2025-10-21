using Microsoft.AspNetCore.Mvc;
using Prog6212_POE.ViewModel;

namespace Prog6212_POE.Controllers
{
    public class TrackController : Controller
    {
        public IActionResult Index()
        {
            // Get all claims (in real app, filter by user)
            var userClaims = SubmitController.GetClaims();
            return View(userClaims);
        }

        public IActionResult Details(int id)
        {
            var claim = SubmitController.GetClaimById(id);
            if (claim == null)
            {
                return NotFound();
            }
            return View(claim);
        }
    }
}
