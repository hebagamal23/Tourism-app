using System.ComponentModel.DataAnnotations;

namespace Tourism_project.Models
{
    public class Hotel
    {
        [Key]
        public int HotelId { get; set; }  // المفتاح الأساسي
        public string Name { get; set; }
        public string Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public int Stars { get; set; }
        public int LocationId { get; set; }
        public double DistanceFromLocation { get; set; }
        public int Rating { get; set; }
        public string Description { get; set; }
        public DateTime EstablishedDate { get; set; }

        public decimal PricePerNight { get; set; } // سعر الليلة
        public int MaxRooms { get; set; }
        public Location Location { get; set; }
        public ICollection<Room> Rooms { get; set; }
        public ICollection<HotelService> HotelServices { get; set; }
        
        public ICollection<MediaHotels> Media { get; set; }
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();


        // إضافة العلاقة مع HotelRestriction
        public ICollection<HotelRestriction> Restrictions { get; set; }  // إضافة هذا السطر

    }
}
