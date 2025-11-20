using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prog6212_POE.Data;
using Prog6212_POE.Models;

namespace Prog6212_POE.Controllers
{
    [Authorize(Roles = "Lecturer")] // Direct role instead of policy
    public class SubmitController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubmitController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ClaimModel
            {
                // Rate will be auto-populated from User.HourlyRate via the ClaimModel property
            };

            ViewBag.UserFullName = user.FullName;
            ViewBag.UserHourlyRate = user.HourlyRate;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(ClaimModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Monthly validation (180 hours max)
                    var monthlyHours = await _context.Claims
                        .Where(c => c.UserId == user.Id &&
                                   c.ClaimDate.Month == DateTime.Now.Month &&
                                   c.ClaimDate.Year == DateTime.Now.Year &&
                                   c.Status != "Rejected")
                        .SumAsync(c => c.HoursWorked);

                    if (monthlyHours + model.HoursWorked > 180)
                    {
                        ModelState.AddModelError("HoursWorked",
                            $"Maximum monthly hours (180) exceeded. You have {monthlyHours} hours this month. Remaining: {180 - monthlyHours} hours.");

                        ViewBag.UserFullName = user.FullName;
                        ViewBag.UserHourlyRate = user.HourlyRate;
                        return View("Index", model);
                    }

                    // File validation
                    if (model.Receipt != null && model.Receipt.Length > 0)
                    {
                        if (model.Receipt.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("Receipt", "File size must be less than 5MB");
                            ViewBag.UserFullName = user.FullName;
                            ViewBag.UserHourlyRate = user.HourlyRate;
                            return View("Index", model);
                        }

                        var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
                        var fileExtension = Path.GetExtension(model.Receipt.FileName).ToLower();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("Receipt", "Only PDF, DOCX, and XLSX files are allowed");
                            ViewBag.UserFullName = user.FullName;
                            ViewBag.UserHourlyRate = user.HourlyRate;
                            return View("Index", model);
                        }

                        model.FileName = $"{Guid.NewGuid()}_{model.Receipt.FileName}";
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", model.FileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Receipt.CopyToAsync(stream);
                        }
                    }

                    // Create claim with user data from HR
                    var claim = new ClaimModel
                    {
                        UserId = user.Id,
                        Contract = model.Contract,
                        ClaimDate = DateTime.Now,
                        Category = model.Category,
                        HoursWorked = model.HoursWorked,
                        FileName = model.FileName,
                        Status = "Pending"
                    };

                    _context.Claims.Add(claim);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Claim #{claim.Id} submitted successfully! Total amount: R {claim.Amount:F2}";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while submitting your claim. Please try again.");
                    System.Diagnostics.Debug.WriteLine($"Claim submission error: {ex.Message}");
                }
            }

            // If we got this far, something failed
            ViewBag.UserFullName = user.FullName;
            ViewBag.UserHourlyRate = user.HourlyRate;
            return View("Index", model);
        }

        [HttpPost]
        public async Task<JsonResult> CalculateAmount(int hoursWorked)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { error = "User not found" });
            }

            var amount = hoursWorked * user.HourlyRate;

            // Monthly validation
            var monthlyHours = await _context.Claims
                .Where(c => c.UserId == user.Id &&
                           c.ClaimDate.Month == DateTime.Now.Month &&
                           c.ClaimDate.Year == DateTime.Now.Year &&
                           c.Status != "Rejected")
                .SumAsync(c => c.HoursWorked);

            var remainingHours = 180 - monthlyHours;
            var wouldExceed = (monthlyHours + hoursWorked) > 180;
            var canSubmit = hoursWorked > 0 && hoursWorked <= 12 && !wouldExceed;

            return Json(new
            {
                amount = amount,
                formattedAmount = $"R {amount:F2}",
                hourlyRate = user.HourlyRate,
                monthlyHours = monthlyHours,
                remainingHours = remainingHours,
                wouldExceed = wouldExceed,
                canSubmit = canSubmit
            });
        }


        private async Task SendClaimNotification(ClaimModel claim)
        {
            // This is where you can implement email notifications
            Console.WriteLine($"New claim submitted: #{claim.Id} by user for amount R {claim.Amount:F2}");

            // In a real application, you would:
            // 1. Send email to coordinator
            // 2. Send notification to manager
            // 3. Log to system notifications
        }

       

        // Remove the static methods and replace with database calls
        public static List<ClaimModel> GetClaims()
        {
            // This should be replaced with proper database calls
            return new List<ClaimModel>();
        }

        public static ClaimModel GetClaimById(int id)
        {
            // This should be replaced with proper database calls
            return null;
        }
    }
}