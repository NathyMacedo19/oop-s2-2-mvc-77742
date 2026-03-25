using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodSafety.Domain
{
    public class Premises
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Town { get; set; }
        public string RiskRating { get; set; } // Low/Medium/High

        // Navigation property
        public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    }
}