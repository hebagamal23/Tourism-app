using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tourism_project.Models
{
   
    public class Booking
    {
        [Key]
        public int BookingId { get; set; } 
        public int TouristId { get; set; }
        public int? RoomId { get; set; }
        public int PaymentMethodId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int NumberOfGuests { get; set; } // ✅ عدد الأفراد في الحجز
                                                // عمود وقت الدفع
        public DateTime? PaymentTime { get; set; }  //nullable DateTime for payment time


        public BookingStatus Status { get; set; } = BookingStatus.Pending; // حالة الحجز


        [ForeignKey("TouristId")]
        public Tourism Tourist { get; set; }
        
        [ForeignKey("RoomId")]
        public Room Room { get; set; } 

        [ForeignKey("PaymentMethodId")]
       public PaymentMethod PaymentMethod { get; set; }
        
        public ICollection<BookingActivity> BookingActivities { get; set; }

        
        public Payment Payment { get; set; }

        // Enum لحالة الحجز
        public enum BookingStatus
        {
            Confirmed,
            Pending,
            Cancelled,
            Completed,
            Expired
        }
    }


}
