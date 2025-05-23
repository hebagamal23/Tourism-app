namespace Tourism_project.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; } 

        public string? ImageUrl { get; set; } 


        public string description { get; set; }

        
        public ICollection<TourismTypeLocation> TourismTypeLocations { get; set; }

       
        public ICollection<Hotel> Hotels { get; set; }

        public ICollection<LocationActivity> locationActivities { get; set; }
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();



    }
}
