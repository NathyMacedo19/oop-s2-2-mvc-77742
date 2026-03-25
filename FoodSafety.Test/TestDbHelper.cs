using Microsoft.EntityFrameworkCore;
using FoodSafety.MVC.Data;
using FoodSafety.Domain;

namespace FoodSafety.Test
{
    public static class TestDbHelper
    {
        public static AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        public static async Task<AppDbContext> SeedTestDataAsync()
        {
            var dbContext = CreateInMemoryDbContext();  // ← Type is AppDbContext, not object

            var premises = new Premises
            {
                Id = 1,
                Name = "Test Restaurant",
                Address = "123 Main St",
                Town = "Bournemouth",
                RiskRating = "High"
            };
            dbContext.Premises.Add(premises);  // ← Works because dbContext is AppDbContext

            var inspection = new Inspection
            {
                Id = 1,
                PremisesId = 1,
                InspectionDate = DateTime.Today,
                Score = 45,
                Outcome = "Fail",
                Notes = "Test inspection"
            };
            dbContext.Inspections.Add(inspection);  // ← Works

            var followup = new FollowUp
            {
                Id = 1,
                InspectionId = 1,
                DueDate = DateTime.Today.AddDays(-1),
                Status = "Open",
                ClosedDate = null
            };
            dbContext.FollowUps.Add(followup);  // ← Works

            await dbContext.SaveChangesAsync();

            return dbContext;
        }
    }
}