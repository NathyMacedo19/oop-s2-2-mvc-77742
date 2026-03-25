using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodSafety.MVC.Data;
using FoodSafety.MVC.ViewModels;
using FoodSafety.Domain;

namespace FoodSafety.MVC.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(AppDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? town, string? riskRating)
        {
            _logger.LogInformation("Accessing Dashboard with filters - Town: {Town}, RiskRating: {RiskRating}",
                town ?? "All", riskRating ?? "All");

            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            // Start with base query
            var inspectionsQuery = _context.Inspections
                .Include(i => i.Premises)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(town))
            {
                inspectionsQuery = inspectionsQuery.Where(i => i.Premises.Town == town);
                _logger.LogInformation("Filtering by town: {Town}", town);
            }

            if (!string.IsNullOrEmpty(riskRating))
            {
                inspectionsQuery = inspectionsQuery.Where(i => i.Premises.RiskRating == riskRating);
                _logger.LogInformation("Filtering by risk rating: {RiskRating}", riskRating);
            }

            // Get unique filter values for dropdowns
            var towns = await _context.Premises
                .Select(p => p.Town)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            var riskRatings = await _context.Premises
                .Select(p => p.RiskRating)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                InspectionsThisMonth = await inspectionsQuery
                    .CountAsync(i => i.InspectionDate >= firstDayOfMonth),

                FailedInspectionsThisMonth = await inspectionsQuery
                    .CountAsync(i => i.InspectionDate >= firstDayOfMonth && i.Outcome == "Fail"),

                OverdueFollowUps = await _context.FollowUps
                    .CountAsync(f => f.Status == "Open" && f.DueDate < today),

                Towns = towns,
                RiskRatings = riskRatings,
                SelectedTown = town,
                SelectedRiskRating = riskRating
            };

            return View(model);
        }
    }
}