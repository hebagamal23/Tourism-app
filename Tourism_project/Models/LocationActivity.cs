using System.Diagnostics;

namespace Tourism_project.Models
{
    public class LocationActivity
    {

        public int LocationId { get; set; }
        public Location Location { get; set; }
        public int ActivityId { get; set; }
        public ACtivity Activity { get; set; }
    }
}
