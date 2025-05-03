namespace Tourism_project.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; } // اسم المكان
        public string? ImageUrl { get; set; } // خاصية الصورة (يمكن أن تكون URL أو Base64)

        public string description { get; set; }

        // الأماكن التابعة لنوع السياحة (علاقة many-to-many عبر TourismTypeLocation)
        public ICollection<TourismTypeLocation> TourismTypeLocations { get; set; }

        // الفنادق المتاحة في هذا المكان (علاقة one-to-many بين Location و Hotel)
        public ICollection<Hotel> Hotels { get; set; }


        // العلاقة many-to-many مع Transportation عبر جدول TransportationLocation

       // public ICollection<TransportationLocation> TransportationLocations { get; set; }
        public ICollection<LocationActivity> locationActivities { get; set; }
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();



    }
}
