using FoodSafety.Domain;
using FoodSafety.MVC.Controllers;
using FoodSafety.MVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FoodSafety.Test
{
    public class FollowupsControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewWithListOfFollowUps()
        {
            // ==========================================
            // Create in-memory database
            // ==========================================
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new AppDbContext(options);

            // ==========================================
            // Create required related data first
            // ==========================================

            // FollowUp precisa de um Inspection
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
                Score = 85,
                Outcome = "Pass",
                Notes = "Test inspection"
            };
            dbContext.Inspections.Add(inspection);

            await dbContext.SaveChangesAsync();

            // ==========================================
            // Now add the FollowUp
            // ==========================================
            var followUp = new FollowUp
            {
                Id = 1,
                InspectionId = 1,  // Must reference existing inspection
                DueDate = DateTime.Today.AddDays(7),
                Status = "Open",
                ClosedDate = null
            };
            dbContext.FollowUps.Add(followUp);

            await dbContext.SaveChangesAsync();

            // ==========================================
            // Create mock logger
            // ==========================================
            var loggerMock = new Mock<ILogger<FollowupsController>>();

            // ==========================================
            // Create controller
            // ==========================================
            var controller = new FollowupsController(dbContext, loggerMock.Object);

            // ==========================================
            // Act
            // ==========================================
            var result = await controller.Index();

            // ==========================================
            // Assert
            // ==========================================
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<FollowUp>>(viewResult.Model);

            // Now this should pass
            Assert.Single(model);
            Assert.Equal(1, model.First().Id);
            Assert.Equal("Open", model.First().Status);
        }
    }
}
