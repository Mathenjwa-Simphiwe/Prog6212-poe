using Microsoft.AspNetCore.Mvc;
using Prog6212_POE.Controllers;
using Prog6212_POE.ViewModel;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace Prog6212_POE.Tests
{
    public class SubmitTest : IDisposable
    {
        private readonly SubmitController _controller;

        public SubmitTest()
        {
            _controller = new SubmitController();
            // Clear any existing claims before each test
            ClearTestData();
        }

        public void Dispose()
        {
            ClearTestData();
        }

        private void ClearTestData()
        {
            // Access the private static field via reflection to clear it
            var claimsField = typeof(SubmitController).GetField("_claims",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var nextIdField = typeof(SubmitController).GetField("_nextId",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            claimsField?.SetValue(null, new List<ClaimViewModel>());
            nextIdField?.SetValue(null, 1);
        }

        [Fact]
        public void SubmitClaim_ValidModel_ShouldAddClaimAndRedirect()
        {
            // Arrange
            var model = new ClaimViewModel
            {
                Contract = "Prog-2025",
                ClaimDate = DateTime.Now,
                Category = "labor",
                HoursWorked = 10,
                Rate = 450.00m
            };

            // Act
            var result = _controller.SubmitClaim(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            var claims = SubmitController.GetClaims();
            Assert.Single(claims);
            Assert.Equal(1, claims[0].Id);
            Assert.Equal("Pending", claims[0].Status);
            Assert.Equal(4500.00m, claims[0].Amount);
        }

        [Fact]
        public void SubmitClaim_InvalidModel_ShouldReturnViewWithModel()
        {
            // Arrange
            var model = new ClaimViewModel
            {
                Contract = "", // Invalid - empty contract
                ClaimDate = DateTime.Now,
                Category = "labor",
                HoursWorked = 10,
                Rate = 450.00m
            };
            _controller.ModelState.AddModelError("Contract", "Contract is required");

            // Act
            var result = _controller.SubmitClaim(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ViewName);
            Assert.Equal(model, result.Model);
        }
        [Fact]
        public void SubmitClaim_WithValidFile_ShouldProcessFileCorrectly()
        {
            // Arrange
            var file = new FormFile(
                baseStream: new MemoryStream(Encoding.UTF8.GetBytes("Test file content")),
                baseStreamOffset: 0,
                length: 20,
                name: "Receipt", // Should match the property name
                fileName: "test.pdf"
            );

            var model = new ClaimViewModel
            {
                Contract = "Prog-2025",
                ClaimDate = DateTime.Now,
                Category = "labor",
                HoursWorked = 8,
                Rate = 400.00m,
                Receipt = file
            };

            // Act
            var result = _controller.SubmitClaim(model);

            // Assert - Test that the claim was accepted with a file
            var claims = SubmitController.GetClaims();
            Assert.Single(claims);
            Assert.NotNull(claims[0].Receipt); // File object should be present
            Assert.Equal("test.pdf", claims[0].Receipt.FileName); // File name should be accessible via Receipt
            Assert.Equal(20, claims[0].Receipt.Length); // File size should be preserved
        }

        [Fact]
        public void SubmitClaim_WithLargeFile_ShouldReturnValidationError()
        {
            // Arrange
            var largeFile = new FormFile(
                baseStream: new MemoryStream(new byte[6 * 1024 * 1024]), // 6MB file
                baseStreamOffset: 0,
                length: 6 * 1024 * 1024,
                name: "Data",
                fileName: "large.pdf"
            );

            var model = new ClaimViewModel
            {
                Contract = "Prog-2025",
                ClaimDate = DateTime.Now,
                Category = "labor",
                HoursWorked = 8,
                Rate = 400.00m,
                Receipt = largeFile
            };

            // Act
            var result = _controller.SubmitClaim(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(_controller.ModelState.Values, v => v.Errors.Any(e => e.ErrorMessage.Contains("File size must be less than 5MB")));
        }

        [Fact]
        public void SubmitClaim_WithInvalidFileType_ShouldReturnValidationError()
        {
            // Arrange
            var invalidFile = new FormFile(
                baseStream: new MemoryStream(Encoding.UTF8.GetBytes("Test content")),
                baseStreamOffset: 0,
                length: 14,
                name: "Data",
                fileName: "test.exe" // Invalid file type
            );

            var model = new ClaimViewModel
            {
                Contract = "Prog-2025",
                ClaimDate = DateTime.Now,
                Category = "labor",
                HoursWorked = 8,
                Rate = 400.00m,
                Receipt = invalidFile
            };

            // Act
            var result = _controller.SubmitClaim(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(_controller.ModelState.Values, v => v.Errors.Any(e => e.ErrorMessage.Contains("Only PDF, DOCX, and XLSX files are allowed")));
        }

        [Fact]
        public void GetClaims_ShouldReturnAllClaims()
        {
            // Arrange
            var model1 = new ClaimViewModel { Contract = "Prog-2025", ClaimDate = DateTime.Now, Category = "labor", HoursWorked = 5, Rate = 400.00m };
            var model2 = new ClaimViewModel { Contract = "Research-2024", ClaimDate = DateTime.Now, Category = "equipment", HoursWorked = 3, Rate = 300.00m };

            _controller.SubmitClaim(model1);
            _controller.SubmitClaim(model2);

            // Act
            var claims = SubmitController.GetClaims();

            // Assert
            Assert.Equal(2, claims.Count);
            Assert.Contains(claims, c => c.Contract == "Prog-2025");
            Assert.Contains(claims, c => c.Contract == "Research-2024");
        }

        [Fact]
        public void GetClaimById_ExistingId_ShouldReturnClaim()
        {
            // Arrange
            var model = new ClaimViewModel { Contract = "Prog-2025", ClaimDate = DateTime.Now, Category = "labor", HoursWorked = 5, Rate = 400.00m };
            _controller.SubmitClaim(model);

            // Act
            var claim = SubmitController.GetClaimById(1);

            // Assert
            Assert.NotNull(claim);
            Assert.Equal(1, claim.Id);
            Assert.Equal("Prog-2025", claim.Contract);
        }

        [Fact]
        public void GetClaimById_NonExistingId_ShouldReturnNull()
        {
            // Act
            var claim = SubmitController.GetClaimById(999);

            // Assert
            Assert.Null(claim);
        }
    }
}
