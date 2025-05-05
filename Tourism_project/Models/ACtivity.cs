namespace Tourism_project.Models
{
    public class ACtivity
    {

        public int ActivityId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string? MoreDescription { get; set; }
        public float Price { get; set; }
        public string? ImageUrl { get; set; }
        public int DurationHours { get; set; }
        public ICollection<LocationActivity> locationActivities { get; set; }
        public ICollection<BookingActivity> BookingActivities { get; set; }
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}

