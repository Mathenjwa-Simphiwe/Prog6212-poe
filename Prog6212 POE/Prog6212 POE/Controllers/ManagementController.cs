using Microsoft.AspNetCore.Mvc;
using Prog6212_POE.Models;

namespace Prog6212_POE.Controllers
{
    public class ManagementController : Controller
    {
        public IActionResult Index()
        {
            // Get all pending claims
            var pendingClaims = SubmitController.GetClaims()
                .Where(c => c.Status == "Pending")
                .OrderBy(c => c.ClaimDate)
                .ToList();

            return View(pendingClaims);
        }

        [HttpPost]
        public IActionResult ApproveClaim(int id)
        {
            var claim = SubmitController.GetClaimById(id);
            if (claim != null)
            {
                claim.Status = "Approved";
                TempData["SuccessMessage"] = $"Claim #{id} has been approved successfully.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RejectClaim(int id)
        {
            var claim = SubmitController.GetClaimById(id);
            if (claim != null)
            {
                claim.Status = "Rejected";
                TempData["SuccessMessage"] = $"Claim #{id} has been rejected.";
            }
            return RedirectToAction("Index");
        }

        public IActionResult ClaimDetails(int id)
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