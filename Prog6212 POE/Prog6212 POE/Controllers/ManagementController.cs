using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prog6212_POE.Data;
using Prog6212_POE.Models;

namespace Prog6212_POE.Controllers
{
    [Authorize(Policy = "CoordinatorOnly")]
    public class ManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ManagementController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            // Set session for admin access tracking
            HttpContext.Session.SetString("LastAccess", DateTime.Now.ToString());
            HttpContext.Session.SetString("UserRole", "Coordinator");

            var pendingClaims = await _context.Claims
                .Include(c => c.User)
                .Where(c => c.Status == "Pending")
                .OrderBy(c => c.ClaimDate)
                .ToListAsync();

            return View(pendingClaims);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveClaim(int id, string notes)
        {
            // Check session
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole")))
            {
                return RedirectToAction("Index", "Home");
            }

            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim == null)
            {
                return NotFound();
            }

            // Automated validation
            var validationResult = await ValidateClaim(claim);
            if (!validationResult.IsValid)
            {
                TempData["Error"] = validationResult.ErrorMessage;
                return RedirectToAction("Index");
            }

            claim.Status = "Approved";
            await _context.SaveChangesAsync();

            // Automated approval notification
            await SendApprovalNotification(claim);

            TempData["SuccessMessage"] = $"Claim #{id} has been approved successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RejectClaim(int id, string notes)
        {
            // Check session
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole")))
            {
                return RedirectToAction("Index", "Home");
            }

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }

            claim.Status = "Rejected";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Claim #{id} has been rejected.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ClaimDetails(int id)
        {
            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim == null)
            {
                return NotFound();
            }
            return View(claim);
        }

        // Automated claim validation
        private async Task<ValidationResult> ValidateClaim(ClaimModel claim)
        {
            // Predefined validation criteria
            if (claim.HoursWorked > 12)
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Hours worked exceed maximum allowed (12 hours)." };
            }

            // Check for duplicate claims in same period
            var recentClaims = await _context.Claims
                .Where(c => c.Id != claim.Id && c.UserId == claim.UserId &&
                           c.ClaimDate.Month == claim.ClaimDate.Month)
                .CountAsync();

            if (recentClaims > 5) // Maximum 5 claims per contract per month
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Maximum claims per contract for this month reached." };
            }

            return new ValidationResult { IsValid = true };
        }

        private async Task SendApprovalNotification(ClaimModel claim)
        {
            // Automated notification for approved claims
            Console.WriteLine($"Claim #{claim.Id} approved. Amount: R {claim.Amount:F2}");

            // In real application:
            // 1. Notify lecturer
            // 2. Update HR system
            // 3. Generate payment request
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }
}