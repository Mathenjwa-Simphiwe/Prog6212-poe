using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Role { get; set; } // Lecturer, Coordinator, Manager, HR
    public string Department { get; set; }
    public decimal HourlyRate { get; set; } // Added hourly rate
    public DateTime DateCreated { get; set; } = DateTime.Now;

    public virtual ICollection<ClaimModel> Claims { get; set; }
}