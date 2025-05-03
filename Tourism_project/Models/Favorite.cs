using System.Diagnostics;

namespace Tourism_project.Models
{
    public class Favorite
    {
        public int FavoriteId { get; set; } // المفتاح الأساسي
        public int UserId { get; set; } // معرف المستخدم

        public int ItemId { get; set; } // معرف العنصر المفضل
        public string ItemType { get; set; } // نوع العنصر ("Hotel", "Activity", "Location", "TourismType")

        public DateTime AddedAt { get; set; } = DateTime.Now; // وقت الإضافة

        // 🔹 علاقات مع الجداول الأخرى (علاقة اختيارية لكل نوع)
        public Hotel? Hotel { get; set; }
        public ACtivity? Activity { get; set; }
        public Location? Location { get; set; }
        public TourismType? TourismType { get; set; }

        public Tourism User { get; set; } // علاقة مع المستخدم
    }
}
