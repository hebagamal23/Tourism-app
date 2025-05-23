namespace Tourism_project.Models
{
    public class TourismType
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string description { get; set; }
        public Boolean is_active { get; set; }
        
       
        public string? ImageUrl { get; set; }

        
        public ICollection<TourismTypeLocation> TourismTypeLocations { get; set; }

        
        public ICollection<TouristTourismType> TouristTourismTypes { get; set; }
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
