using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Prog6212_POE.Models
{
    public class ClaimModel
    {
        public int Id { get; set; }
        public string Contract { get; set; }
        public DateTime ClaimDate { get; set; }
        public string Category { get; set; }
        public int HoursWorked { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; } = "Pending";

        // For file upload - not stored in the list
        [BindNever]
        public IFormFile Receipt { get; set; }
    }
}