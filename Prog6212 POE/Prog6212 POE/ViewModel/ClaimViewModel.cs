namespace Prog6212_POE.ViewModel
{
    public class ClaimViewModel
    {
        public int Id { get; set; }
        public string Contract { get; set; }
        public DateTime ClaimDate { get; set; }
        public string Category { get; set; }
        public int HoursWorked { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public IFormFile Receipt { get; set; }
        
        public string Status { get; set; } = "Pending";
    }
}
