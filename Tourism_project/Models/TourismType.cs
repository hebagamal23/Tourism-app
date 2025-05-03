namespace Tourism_project.Models
{
    public class TourismType
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string description { get; set; }
        public Boolean is_active { get; set; }
        
        // خاصية الصورة (يمكن أن تكون URL أو Base64)
        public string? ImageUrl { get; set; }

        // الأماكن التابعة لنوع السياحة (علاقة many-to-many عبر TourismTypeLocation)
        public ICollection<TourismTypeLocation> TourismTypeLocations { get; set; }

        // علاقة متعددة إلى متعددة
        public ICollection<TouristTourismType> TouristTourismTypes { get; set; }
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
