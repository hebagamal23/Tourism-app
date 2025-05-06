using System.ComponentModel.DataAnnotations.Schema;

namespace Tourism_project.Models
{
    public class AddActivityToCart
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int ActivityId { get; set; }

        [ForeignKey("ActivityId")]
        public ACtivity Activity { get; set; }

        public string ActivityName { get; set; }

        public decimal ActivityPrice { get; set; }

        public string ActivityImageUrl { get; set; }

        public int LocationId { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;
        public int NumberOfGuests { get; set; } // ✅ عدد الأشخاص المراد حجزهم
    }

}
