using System.Collections.Generic;

namespace FoodSafety.MVC.ViewModels
{
    public class DashboardViewModel
    {
        public int InspectionsThisMonth { get; set; }
        public int FailedInspectionsThisMonth { get; set; }
        public int OverdueFollowUps { get; set; }
        public List<string> Towns { get; set; } = new List<string>();
        public List<string> RiskRatings { get; set; } = new List<string>();
        public string? SelectedTown { get; set; }
        public string? SelectedRiskRating { get; set; }
    }
}
