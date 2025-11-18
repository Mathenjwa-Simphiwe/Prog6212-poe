using Microsoft.AspNetCore.Identity;
using Prog6212_POE.Models;

namespace Prog6212_POE.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Create HR user if doesn't exist
            if (!userManager.Users.Any(u => u.Role == "HR"))
            {
                var hrUser = new ApplicationUser
                {
                    UserName = "hr@university.com",
                    Email = "hr@university.com",
                    FirstName = "HR",
                    LastName = "Administrator",
                    Role = "HR",
                    Department = "Human Resources",
                    HourlyRate = 0
                };

                var result = await userManager.CreateAsync(hrUser, "HRpassword123!");
                if (result.Succeeded)
                {
                    Console.WriteLine("HR user created successfully.");
                }
            }

            // Create sample lecturer if none exist
            if (!userManager.Users.Any(u => u.Role == "Lecturer"))
            {
                var lecturer = new ApplicationUser
                {
                    UserName = "lecturer@university.com",
                    Email = "lecturer@university.com",
                    FirstName = "John",
                    LastName = "Smith",
                    Role = "Lecturer",
                    Department = "Computer Science",
                    HourlyRate = 350.00m
                };

                await userManager.CreateAsync(lecturer, "Lecturer123!");
            }

            // Create sample coordinator if none exist
            if (!userManager.Users.Any(u => u.Role == "Coordinator"))
            {
                var coordinator = new ApplicationUser
                {
                    UserName = "coordinator@university.com",
                    Email = "coordinator@university.com",
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    Role = "Coordinator",
                    Department = "Computer Science",
                    HourlyRate = 0
                };

                await userManager.CreateAsync(coordinator, "Coordinator123!");
            }

            await context.SaveChangesAsync();
        }
    }
}