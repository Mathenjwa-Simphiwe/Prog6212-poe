using Microsoft.AspNetCore.Mvc;
using Prog6212_POE.ViewModel;

namespace Prog6212_POE.Controllers
{
    public class SubmitController : Controller
    {
        private static List<ClaimViewModel> _claims = new List<ClaimViewModel>();
        private static int _nextId = 1;

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitClaim(ClaimViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Server-side file validation
                if (model.Receipt != null && model.Receipt.Length > 0)
                {
                    // Check file size
                    if (model.Receipt.Length > 5 * 1024 * 1024) // 5MB
                    {
                        ModelState.AddModelError("Receipt", "File size must be less than 5MB");
                        return View("Index", model);
                    }

                    // Check file type
                    var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
                    var fileExtension = Path.GetExtension(model.Receipt.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("Receipt", "Only PDF, DOCX, and XLSX files are allowed");
                        return View("Index", model);
                    }

                    model.FileName = model.Receipt.FileName;

                    // In a real application, you would save the file here:
                    // await SaveFileToServer(model.Receipt, model.Id);
                }

                model.Id = _nextId++;
                model.Amount = model.HoursWorked * model.Rate;
                model.Status = "Pending";

                _claims.Add(model);
                TempData["Success"] = $"Claim #{model.Id} submitted successfully!";
                return RedirectToAction("Index");
            }

            return View("Index", model);
        }

        // Method to get claims for other controllers
        public static List<ClaimViewModel> GetClaims()
        {
            return _claims;
        }

        public static ClaimViewModel GetClaimById(int id)
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