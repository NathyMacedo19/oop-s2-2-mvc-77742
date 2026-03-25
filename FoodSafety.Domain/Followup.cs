using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodSafety.Domain
{
    public class Followup
    {
        public int Id { get; set; }
        public int InspectionId { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ClosedDate { get; set; }
        public Inspection Inspection { get; set; } = null!;
    }
}