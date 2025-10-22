using Microsoft.AspNetCore.Mvc;
using Prog6212_POE.Controllers;
using Prog6212_POE.ViewModel;

namespace Prog6212_POE.Tests
{
    public class ManagementTest : IDisposable
    {
        private readonly ManagementController _controller;
        private readonly SubmitController _submitController;

        public ManagementTest()
        {
            _controller = new ManagementController();
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
        public void Index_ShouldReturnOnlyPendingClaims()
        {
            // Arrange
            var pendingClaim = new ClaimViewModel { Contract = "Prog-2025", ClaimDate = DateTime.Now, Category = "labor", HoursWorked = 5, Rate = 400.00m };
            var approvedClaim = new ClaimViewModel { Contract = "Research-2024", ClaimDate = DateTime.Now, Category = "equipment", HoursWorked = 3, Rate = 300.00m };

            _submitController.SubmitClaim(pendingClaim);
            _submitController.SubmitClaim(approvedClaim);

            // Manually approve the second claim
            SubmitController.UpdateClaimStatus(2, "Approved");

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = result.Model as List<ClaimViewModel>;
            Assert.NotNull(model);
            Assert.Single(model);
            Assert.All(model, claim => Assert.Equal("Pending", claim.Status));
        }

        [Fact]
        public void Index_NoPendingClaims_ShouldReturnEmptyList()
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
        public void ApproveClaim_ShouldUpdateStatusAndRedirect()
        {
            // Arrange
            var claim = new ClaimViewModel { Contract = "Prog-2025", ClaimDate = DateTime.Now, Category = "labor", HoursWorked = 5, Rate = 400.00m };
            _submitController.SubmitClaim(claim);
            var claimId = 1;

            // Act
            var result = _controller.ApproveClaim(claimId) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            var updatedClaim = SubmitController.GetClaimById(claimId);
            Assert.NotNull(updatedClaim);
            Assert.Equal("Approved", updatedClaim.Status);
        }

        [Fact]
        public void RejectClaim_ShouldUpdateStatusAndRedirect()
        {
            // Arrange
            var claim = new ClaimViewModel { Contract = "Prog-2025", ClaimDate = DateTime.Now, Category = "labor", HoursWorked = 5, Rate = 400.00m };
            _submitController.SubmitClaim(claim);
            var claimId = 1;

            // Act
            var result = _controller.RejectClaim(claimId) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            var updatedClaim = SubmitController.GetClaimById(claimId);
            Assert.NotNull(updatedClaim);
            Assert.Equal("Rejected", updatedClaim.Status);
        }

        [Fact]
        public void ApproveClaim_NonExistingId_ShouldRedirectWithoutError()
        {
            // Act
            var result = _controller.ApproveClaim(999) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public void ClaimDetails_ExistingId_ShouldReturnClaim()
        {
            // Arrange
            var claim = new ClaimViewModel { Contract = "Prog-2025", ClaimDate = DateTime.Now, Category = "labor", HoursWorked = 5, Rate = 400.00m };
            _submitController.SubmitClaim(claim);
            var claimId = 1;

            // Act
            var result = _controller.ClaimDetails(claimId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = result.Model as ClaimViewModel;
            Assert.NotNull(model);
            Assert.Equal(claimId, model.Id);
        }

        [Fact]
        public void ClaimDetails_NonExistingId_ShouldReturnNotFound()
        {
            // Act
            var result = _controller.ClaimDetails(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
