using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Prog6212_POE.Models
{
    public class ClaimModel
    {
        public int Id { get; set; }

        // Link to lecturer - this is critical
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string Contract { get; set; }
        public DateTime ClaimDate { get; set; }
        public string Category { get; set; }
        public int HoursWorked { get; set; }

        // This should come from HR data, not user input
        [NotMapped] // Don't store in database - calculate from User.HourlyRate
        public decimal Rate => User?.HourlyRate ?? 0;

        public decimal Amount => HoursWorked * Rate;
        public string FileName { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime? ApprovedDate { get; set; }
        public string ApprovedBy { get; set; }

        // For file upload - not stored in database
        [BindNever]
        [NotMapped]
        public IFormFile Receipt { get; set; }
    }
}