using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prog6212_POE.Data;
using Prog6212_POE.Models;

namespace Prog6212_POE.Controllers
{
    [Authorize(Policy = "LecturerOnly")]
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
            var model = new ClaimModel
            {
                Rate = user.HourlyRate // Auto-populate from HR data
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(ClaimModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);

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
                            $"Maximum monthly hours (180) exceeded. You have {monthlyHours} hours this month.");
                        model.Rate = user.HourlyRate; // Reset rate
                        return View("Index", model);
                    }

                    // Server-side file validation
                    if (model.Receipt != null && model.Receipt.Length > 0)
                    {
                        // Check file size
                        if (model.Receipt.Length > 5 * 1024 * 1024) // 5MB
                        {
                            ModelState.AddModelError("Receipt", "File size must be less than 5MB");
                            model.Rate = user.HourlyRate;
                            return View("Index", model);
                        }

                        // Check file type
                        var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
                        var fileExtension = Path.GetExtension(model.Receipt.FileName).ToLower();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("Receipt", "Only PDF, DOCX, and XLSX files are allowed");
                            model.Rate = user.HourlyRate;
                            return View("Index", model);
                        }

                        // Save file information
                        model.FileName = $"{Guid.NewGuid()}_{model.Receipt.FileName}";
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", model.FileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Receipt.CopyToAsync(stream);
                        }
                    }

                    // Auto-calculate amount using user's rate (not input rate)
                    model.Amount = model.HoursWorked * user.HourlyRate;
                    model.Rate = user.HourlyRate; // Use HR-set rate, not user input
                    model.UserId = user.Id;
                    model.Status = "Pending";
                    model.ClaimDate = DateTime.Now;

                    _context.Claims.Add(model);
                    await _context.SaveChangesAsync();

                    // Automated notification
                    await SendClaimNotification(model);

                    TempData["Success"] = $"Claim #{model.Id} submitted successfully! Total amount: R {model.Amount:F2}";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while submitting your claim. Please try again.");
                    // Log the exception
                }
            }

            // If we got this far, something failed; redisplay form
            var user = await _userManager.GetUserAsync(User);
            model.Rate = user.HourlyRate;
            return View("Index", model);
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

        // AJAX endpoint for real-time calculation
        [HttpPost]
        public async Task<JsonResult> CalculateAmount(int hoursWorked)
        {
            var user = await _userManager.GetUserAsync(User);
            var amount = hoursWorked * user.HourlyRate;

            // Check monthly limit
            var monthlyHours = await _context.Claims
                .Where(c => c.UserId == user.Id &&
                           c.ClaimDate.Month == DateTime.Now.Month &&
                           c.ClaimDate.Year == DateTime.Now.Year &&
                           c.Status != "Rejected")
                .SumAsync(c => c.HoursWorked);

            var remainingHours = 180 - monthlyHours;
            var wouldExceed = (monthlyHours + hoursWorked) > 180;

            return Json(new
            {
                amount = amount,
                formattedAmount = $"R {amount:F2}",
                hourlyRate = user.HourlyRate,
                monthlyHours = monthlyHours,
                remainingHours = remainingHours,
                wouldExceed = wouldExceed
            });
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