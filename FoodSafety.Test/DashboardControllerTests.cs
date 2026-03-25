using FoodSafety.Domain;
using FoodSafety.MVC.Controllers;
using FoodSafety.MVC.Data;
using FoodSafety.MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FoodSafety.Test
{
    public class DashboardControllerTests
    {
        [Fact]
        public async Task Dashboard_ReturnsCorrectCounts()
        {
            // Create in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new AppDbContext(options);

            // Seed test data directly in the test
            var premises = new Premises
            {
                Id = 1,
                Name = "Test Restaurant",
                Address = "123 Main St",
                Town = "Bournemouth",
                RiskRating = "High"
            };
            dbContext.Premises.Add(premises);

            var inspection = new Inspection
            {
                Id = 1,
                PremisesId = 1,
                InspectionDate = DateTime.Today,
                Score = 45,
                Outcome = "Fail",
                Notes = "Test inspection"
            };
            dbContext.Inspections.Add(inspection);

            var followUp = new FollowUp
            {
                Id = 1,
                InspectionId = 1,
                DueDate = DateTime.Today.AddDays(-1),
                Status = "Open",
                ClosedDate = null
            };
            dbContext.FollowUps.Add(followUp);

            await dbContext.SaveChangesAsync();

            // Create mock logger
            var loggerMock = new Mock<ILogger<DashboardController>>();

            // Create controller
            var controller = new DashboardController(dbContext, loggerMock.Object);

            // Execute
            var result = await controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DashboardViewModel>(viewResult.Model);

            Assert.Equal(1, model.InspectionsThisMonth);
            Assert.Equal(1, model.FailedInspectionsThisMonth);
            Assert.Equal(1, model.OverdueFollowUps);
            Assert.Contains("Bournemouth", model.Towns);
            Assert.Contains("High", model.RiskRatings);
        }
    }
}