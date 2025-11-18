using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prog6212_POE.Models
{
    public class ClaimModel
    {
        public int Id { get; set; }

        // Link to lecturer
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string Contract { get; set; }
        public DateTime ClaimDate { get; set; }
        public string Category { get; set; }
        public int HoursWorked { get; set; }
        public decimal Rate { get; set; } // This should come from user profile
        public decimal Amount { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; } = "Pending";

        // For file upload - not stored in the list
        [BindNever]
        [NotMapped]
        public IFormFile Receipt { get; set; }
    }
}