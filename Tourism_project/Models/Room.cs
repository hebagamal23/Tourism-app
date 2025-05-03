namespace Tourism_project.Models
{
    public class Room
    {
        public int Id { get; set; }

        public string Name { get; set; } // اسم الغرفة

        public string description { get; set; } // اسم الغرفة

        public RoomType Type { get; set; } // نوع الغرفة (مثل: مزدوجة، فردية)

        public int BedCount { get; set; }
        public int MaxOccupancy { get; set; } // عدد الأسرة
        public double Size { get; set; } // حجم الغرفة بالأمتار المربعة
        public decimal PricePerNight { get; set; } // سعر الليلة
        public bool IsAvailable { get; set; } = true; // هل الغرفة متوفرة؟

        public int HotelId { get; set; } // مفتاح خارجي لربط الغرفة بالفندق
        public Hotel Hotel { get; set; }

       
        // Navigation property for the relationship with Booking

        public ICollection<Booking> Bookings { get; set; } //  A room can have multiple bookings

                                                           // علاقة واحد إلى متعدد مع RoomMedia
        public List<RoomMedia> Media { get; set; } 


        public enum RoomType
        {
            Single,
            Double,
            King_size_Bed,
            Suite
        }

    }
}
