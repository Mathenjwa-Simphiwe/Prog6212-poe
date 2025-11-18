using System.Reflection.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prog6212_POE.Data;
using Prog6212_POE.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Prog6212_POE.Controllers
{
    [Authorize(Policy = "HROnly")]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HRController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // HR Dashboard
        public async Task<IActionResult> Index()
        {
            var stats = new
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalLecturers = await _userManager.Users.CountAsync(u => u.Role == "Lecturer"),
                TotalClaims = await _context.Claims.CountAsync(),
                PendingClaims = await _context.Claims.CountAsync(c => c.Status == "Pending")
            };

            return View(stats);
        }

        // User Management
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // Create User View
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = model.Role,
                    Department = model.Department,
                    HourlyRate = model.HourlyRate
                };

                // Generate temporary password
                var result = await _userManager.CreateAsync(user, model.TemporaryPassword);

                if (result.Succeeded)
                {
                    TempData["Success"] = $"User {model.Email} created successfully!";
                    return RedirectToAction("Users");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // Edit User
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Department = user.Department,
                HourlyRate = user.HourlyRate
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Role = model.Role;
                user.Department = model.Department;
                user.HourlyRate = model.HourlyRate;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction("Users");
                }
            }
            return View(model);
        }

        // Generate Reports
        public async Task<IActionResult> Reports()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateMonthlyReport(DateTime reportDate)
        {
            var claims = await _context.Claims
                .Include(c => c.User)
                .Where(c => c.ClaimDate.Month == reportDate.Month &&
                           c.ClaimDate.Year == reportDate.Year &&
                           c.Status == "Approved")
                .ToListAsync();

            var reportData = claims.GroupBy(c => c.User)
                .Select(g => new MonthlyReportItem
                {
                    LecturerName = g.Key.FirstName + " " + g.Key.LastName,
                    TotalHours = g.Sum(c => c.HoursWorked),
                    TotalAmount = g.Sum(c => c.Amount),
                    ClaimsCount = g.Count()
                }).ToList();

            // Generate PDF
            var pdfBytes = GenerateMonthlyReportPdf(reportData, reportDate);
            return File(pdfBytes, "application/pdf", $"Monthly_Report_{reportDate:yyyy_MM}.pdf");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if user has claims
            var userClaims = await _context.Claims.AnyAsync(c => c.UserId == id);
            if (userClaims)
            {
                TempData["Error"] = $"Cannot delete user {user.Email} because they have existing claims.";
                return RedirectToAction("Users");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = $"User {user.Email} deleted successfully.";
            }
            else
            {
                TempData["Error"] = $"Error deleting user {user.Email}.";
            }

            return RedirectToAction("Users");
        }

        private byte[] GenerateMonthlyReportPdf(List<MonthlyReportItem> data, DateTime reportDate)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text($"Monthly Claims Report - {reportDate:MMMM yyyy}")
                        .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(150);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Lecturer").SemiBold();
                                header.Cell().Text("Total Hours").SemiBold();
                                header.Cell().Text("Claims Count").SemiBold();
                                header.Cell().Text("Total Amount").SemiBold();
                            });

                            foreach (var item in data)
                            {
                                table.Cell().Text(item.LecturerName);
                                table.Cell().Text(item.TotalHours.ToString());
                                table.Cell().Text(item.ClaimsCount.ToString());
                                table.Cell().Text($"R {item.TotalAmount:F2}");
                            }

                            // Total row
                            table.Cell().ColumnSpan(3).Text("TOTAL").SemiBold();
                            table.Cell().Text($"R {data.Sum(x => x.TotalAmount):F2}").SemiBold();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }

    public class CreateUserViewModel
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }
        public decimal HourlyRate { get; set; }
        public string TemporaryPassword { get; set; } = "TempPassword123!";
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }
        public decimal HourlyRate { get; set; }
    }

    public class MonthlyReportItem
    {
        public string LecturerName { get; set; }
        public int TotalHours { get; set; }
        public decimal TotalAmount { get; set; }
        public int ClaimsCount { get; set; }
    }
}