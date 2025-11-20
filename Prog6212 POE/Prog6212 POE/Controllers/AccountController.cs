using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prog6212_POE.Models;

namespace Prog6212_POE.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(email, password, false, false);

                if (result.Succeeded)
                {
                    return LocalRedirect(returnUrl);
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Quick login for testing - REMOVE THIS IN PRODUCTION
        [HttpGet]
        public async Task<IActionResult> QuickLogin(string role)
        {
            // Test users - make sure these exist in your database
            var testUsers = new Dictionary<string, (string email, string password)>
            {
                ["Lecturer"] = ("lecturer@emeris.co.za", "Lecturer123!"),
                ["Coordinator"] = ("coordinator@emeris.co.za", "Coordinator123!"),
                ["Manager"] = ("manager@emeris.co.za", "Manager123!"),
                ["HR"] = ("hr@emeris.co.za", "HRpassword123!")
            };

            if (testUsers.ContainsKey(role))
            {
                var (email, password) = testUsers[role];
                var result = await _signInManager.PasswordSignInAsync(email, password, false, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return Content($"Quick login failed for {role}. Make sure the user exists in the database.");
        }
    }
}