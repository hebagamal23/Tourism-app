
namespace Tourism_project.Dtos.Home
{
    public class RoomDto
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }  // اسم الغرفة
        public int MaxOccupancy { get; set; }  // عدد الأشخاص
        public string RoomImageUrl { get; set; }  // رابط صورة الغرفة
        public string description { get; set; } // اسم الغرفة
        public int BedCount { get; set; }
        public double Size { get; set; } // حجم الغرفة بالأمتار المربعة
        public decimal PricePerNight { get; set; } // سعر الليلة
        public bool IsAvailable { get; set; } = true; // هل الغرفة متوفرة؟
    }
}
