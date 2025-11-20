using Microsoft.AspNetCore.Identity;

namespace Prog6212_POE.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; } // Lecturer, Coordinator, Manager, HR
        public string Department { get; set; }
        public decimal HourlyRate { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<ClaimModel> Claims { get; set; } = new List<ClaimModel>();

        public string FullName => $"{FirstName} {LastName}";
    }
}