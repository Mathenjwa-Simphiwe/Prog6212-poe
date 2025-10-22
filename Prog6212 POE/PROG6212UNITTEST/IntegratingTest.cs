using Prog6212_POE.Controllers;
using Prog6212_POE.ViewModel;

namespace Prog6212_POE.Tests
{
    public class IntegratingTest : IDisposable
    {
        private readonly SubmitController _submitController;
        private readonly ManagementController _managementController;
        private readonly TrackController _trackController;

        public IntegratingTest()
        {
            _submitController = new SubmitController();
            _managementController = new ManagementController();
            _trackController = new TrackController();
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
        public void FullWorkflow_SubmitApproveTrack_ShouldWorkCorrectly()
        {
            // Arrange & Act - Submit a claim
            var claim = new ClaimViewModel
            {
                Contract = "Prog-2025",
                ClaimDate = DateTime.Now,
                Category = "labor",
                HoursWorked = 10,
                Rate = 500.00m
            };
            _submitController.SubmitClaim(claim);

            // Assert - Claim should be in system and pending
            var submittedClaim = SubmitController.GetClaimById(1);
            Assert.NotNull(submittedClaim);
            Assert.Equal("Pending", submittedClaim?.Status);
            Assert.Equal(5000.00m, submittedClaim?.Amount);

            // Act - Approve the claim
            _managementController.ApproveClaim(1);

            // Assert - Claim should be approved
            var approvedClaim = SubmitController.GetClaimById(1);
            Assert.Equal("Approved", approvedClaim?.Status);

            // Act - Check track view
            var trackResult = _trackController.Index() as Microsoft.AspNetCore.Mvc.ViewResult;
            var trackClaims = trackResult?.Model as List<ClaimViewModel>;

            // Assert - Track should show the approved claim
            Assert.NotNull(trackClaims);
            Assert.Single(trackClaims);
            Assert.Equal("Approved", trackClaims[0].Status);

            // Act - Check management view (should be empty since no pending claims)
            var managementResult = _managementController.Index() as Microsoft.AspNetCore.Mvc.ViewResult;
            var pendingClaims = managementResult?.Model as List<ClaimViewModel>;

            // Assert - Management should show no pending claims
            Assert.NotNull(pendingClaims);
            Assert.Empty(pendingClaims);
        }

        [Fact]
        public void MultipleClaims_DifferentStatuses_ShouldBeHandledCorrectly()
        {
            // Arrange & Act
            var claim1 = new ClaimViewModel { Contract = "Prog-2025", ClaimDate = DateTime.Now, Category = "labor", HoursWorked = 5, Rate = 400.00m };
            var claim2 = new ClaimViewModel { Contract = "Research-2024", ClaimDate = DateTime.Now, Category = "equipment", HoursWorked = 3, Rate = 300.00m };
            var claim3 = new ClaimViewModel { Contract = "Teaching-2025", ClaimDate = DateTime.Now, Category = "materials", HoursWorked = 8, Rate = 350.00m };

            _submitController.SubmitClaim(claim1);
            _submitController.SubmitClaim(claim2);
            _submitController.SubmitClaim(claim3);

            // Approve one, reject one, leave one pending
            _managementController.ApproveClaim(1);
            _managementController.RejectClaim(2);

            // Assert
            var allClaims = SubmitController.GetClaims();
            Assert.Equal(3, allClaims.Count);

            var approved = allClaims.First(c => c.Id == 1);
            var rejected = allClaims.First(c => c.Id == 2);
            var pending = allClaims.First(c => c.Id == 3);

            Assert.Equal("Approved", approved.Status);
            Assert.Equal("Rejected", rejected.Status);
            Assert.Equal("Pending", pending.Status);

            // Check management only shows pending claims
            var managementResult = _managementController.Index() as Microsoft.AspNetCore.Mvc.ViewResult;
            var pendingClaims = managementResult?.Model as List<ClaimViewModel>;
            Assert.Single(pendingClaims);
            Assert.Equal(3, pendingClaims[0].Id);
        }
    }
}
