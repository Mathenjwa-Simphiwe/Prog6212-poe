using Microsoft.AspNetCore.Mvc;
using Prog6212_POE.Models;

namespace Prog6212_POE.Controllers
{
    public class SubmitController : Controller
    {
        private static List<ClaimModel> _claims = new List<ClaimModel>();
        private static int _nextId = 1;

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitClaim(ClaimModel model)
        {
            if (ModelState.IsValid)
            {
                // Set ID and calculate amount
                model.Id = _nextId++;
                model.Amount = model.HoursWorked * model.Rate;
                model.Status = "Pending";

                // Handle file name
                if (model.Receipt != null && model.Receipt.Length > 0)
                {
                    model.FileName = model.Receipt.FileName;
                }

                // Store in memory
                _claims.Add(model);

                TempData["Success"] = $"Claim #{model.Id} submitted successfully!";
                return RedirectToAction("Index");
            }

            return View("Index", model);
        }

        // Method to get claims for other controllers
        public static List<ClaimModel> GetClaims()
        {
            return _claims;
        }

        public static ClaimModel GetClaimById(int id)
        {
            return _claims.FirstOrDefault(c => c.Id == id);
        }

        public static void UpdateClaimStatus(int id, string status)
        {
            var claim = _claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = status;
            }
        }
    }
}