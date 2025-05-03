using System.Diagnostics;

namespace Tourism_project.Models
{
    public class BookingActivity
    {
        public int BookingId { get; set; }
        public Booking Booking { get; set; }
        public int ActivityId { get; set; }
        public ACtivity Activity { get; set; }

    }
}
