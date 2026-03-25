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
        public async Task Dashboard_InspectionsThisMonth_ReturnsCorrectCount()
    {
        // ==========================================
        // Create in-memory database
        // ==========================================
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);

        // ==========================================
        // Create premises
        // ==========================================
        var premises1 = new Premises
        {
            Id = 1,
            Name = "Restaurant A",
            Address = "123 Main St",
            Town = "Bournemouth",
            RiskRating = "High"
        };
        dbContext.Premises.Add(premises1);

        var premises2 = new Premises
        {
            Id = 2,
            Name = "Restaurant B",
            Address = "456 Oak St",
            Town = "Poole",
            RiskRating = "Medium"
        };
        dbContext.Premises.Add(premises2);

        await dbContext.SaveChangesAsync();

        // ==========================================
        // Create inspections for this month
        // ==========================================
        var today = DateTime.Today;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        // Inspection 1 - this month (should be counted)
        var inspection1 = new Inspection
        {
            Id = 1,
            PremisesId = 1,
            InspectionDate = firstDayOfMonth.AddDays(5), // This month
            Score = 75,
            Outcome = "Pass",
            Notes = "First inspection"
        };
        dbContext.Inspections.Add(inspection1);

        // Inspection 2 - this month (should be counted)
        var inspection2 = new Inspection
        {
            Id = 2,
            PremisesId = 2,
            InspectionDate = firstDayOfMonth.AddDays(10), // This month
            Score = 90,
            Outcome = "Pass",
            Notes = "Second inspection"
        };
        dbContext.Inspections.Add(inspection2);

        // Inspection 3 - last month (should NOT be counted)
        var inspection3 = new Inspection
        {
            Id = 3,
            PremisesId = 1,
            InspectionDate = firstDayOfMonth.AddMonths(-1).AddDays(5), // Last month
            Score = 60,
            Outcome = "Fail",
            Notes = "Old inspection"
        };
        dbContext.Inspections.Add(inspection3);

        await dbContext.SaveChangesAsync();

        // ==========================================
        // Create mock logger
        // ==========================================
        var loggerMock = new Mock<ILogger<DashboardController>>();

        // ==========================================
        // Create controller
        // ==========================================
        var controller = new DashboardController(dbContext, loggerMock.Object);

        // ==========================================
        // Act - Get dashboard without filters
        // ==========================================
        var result = await controller.Index(null, null);

        // ==========================================
        // Assert
        // ==========================================
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DashboardViewModel>(viewResult.Model);

        // Should have 2 inspections this month
        Assert.Equal(2, model.InspectionsThisMonth);  // Now this should pass
    }

}
}