using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prog6212_POE.Data;
using Prog6212_POE.Models;

namespace Prog6212_POE.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class TrackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TrackController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userClaims = await _context.Claims
                .Where(c => c.UserId == user.Id)
                .OrderByDescending(c => c.ClaimDate)
                .ToListAsync();

            return View(userClaims);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

            if (claim == null)
            {
                return NotFound();
            }
            return View(claim);
        }
    }
}
