using Microsoft.AspNetCore.Mvc;
using Prog6212_POE.Controllers;
using Prog6212_POE.ViewModel;

namespace Prog6212_POE.Tests
{
    public class Trackingtest : IDisposable
    {
        private readonly TrackController _controller;
        private readonly SubmitController _submitController;

        public Trackingtest()
        {
            _controller = new TrackController();
            _submitController = new SubmitController();
            ClearTestData();
        }

        public void Dispose()
        {
            ClearTestData();
        }

        private void ClearTestData()
        {
            var claimsField = typeof(SubmitController).GetField("_claims",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var nextIdField = typeof(SubmitController).GetField("_nextId",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            claimsField?.SetValue(null, new List<ClaimViewModel>());
            nextIdField?.SetValue(null, 1);
        }

        [Fact]
        public void Index_ShouldReturnAllClaims()
        {
            // Arrange
            var claim1 = new ClaimViewModel { Contract = "Prog-2025", ClaimDate = DateTime.Now, Category = "labor", HoursWorked = 5, Rate = 400.00m };
            var claim2 = new ClaimViewModel { Contract = "Research-2024", ClaimDate = DateTime.Now, Category = "equipment", HoursWorked = 3, Rate = 300.00m };

            _submitController.SubmitClaim(claim1);
            _submitController.SubmitClaim(claim2);

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = result.Model as List<ClaimViewModel>;
            Assert.NotNull(model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public void Index_NoClaims_ShouldReturnEmptyList()
        {
            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = result.Model as List<ClaimViewModel>;
            Assert.NotNull(model);
            Assert.Empty(model);
        }

        [Fact]
        public void Details_ExistingId_ShouldReturnClaim()
        {
            // Arrange
            var claim = new ClaimViewModel { Contract = "Prog-2025", ClaimDate = DateTime.Now, Category = "labor", HoursWorked = 5, Rate = 400.00m };
            _submitController.SubmitClaim(claim);
            var claimId = 1;

            // Act
            var result = _controller.Details(claimId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = result.Model as ClaimViewModel;
            Assert.NotNull(model);
            Assert.Equal(claimId, model.Id);
        }

        [Fact]
        public void Details_NonExistingId_ShouldReturnNotFound()
        {
            // Act
            var result = _controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}