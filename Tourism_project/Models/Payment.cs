using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tourism_project.Models
{
    public class Payment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        // عمود وقت الدفع
        public DateTime PaymentTime { get; set; } = DateTime.UtcNow; // وقت الدفع
    
    public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }


        public int PaymentMethodId { get; set; }

        [ForeignKey("PaymentMethodId")]
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = Guid.NewGuid().ToString();
        public string Status { get; set; } = "Pending";


    }
}
