using Prog6212_POE.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace Prog6212_POE.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void ClaimViewModel_ValidData_ShouldBeValid()
        {
            // Arrange
            var claim = new ClaimViewModel
            {
                Contract = "Prog-2025",
                ClaimDate = DateTime.Now,
                Category = "labor",
                HoursWorked = 10,
                Rate = 450.00m
            };

            var context = new ValidationContext(claim);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(claim, context, results, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Theory]
        [InlineData("", "labor", 10, 450)] // Empty contract
        [InlineData("Prog-2025", "", 10, 450)] // Empty category
        [InlineData("Prog-2025", "labor", -5, 450)] // Negative hours
        [InlineData("Prog-2025", "labor", 10, -100)] // Negative rate
        [InlineData("Prog-2025", "labor", 0, 450)] // Zero hours
        public void ClaimViewModel_InvalidData_ShouldFailValidation(string contract, string category, int hours, decimal rate)
        {
            // Arrange
            var claim = new ClaimViewModel
            {
                Contract = contract,
                ClaimDate = DateTime.Now,
                Category = category,
                HoursWorked = hours,
                Rate = rate
            };

            var context = new ValidationContext(claim);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(claim, context, results, true);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(results);
        }

        [Fact]
        public void ClaimViewModel_AmountCalculation_ShouldBeCorrect()
        {
            // Arrange
            var claim = new ClaimViewModel
            {
                HoursWorked = 8,
                Rate = 500.00m
            };

            // Act
            claim.Amount = claim.HoursWorked * claim.Rate;

            // Assert
            Assert.Equal(4000.00m, claim.Amount);
        }

        [Fact]
        public void ClaimViewModel_DefaultStatus_ShouldBePending()
        {
            // Arrange & Act
            var claim = new ClaimViewModel();

            // Assert
            Assert.Equal("Pending", claim.Status);
        }
    }
}
