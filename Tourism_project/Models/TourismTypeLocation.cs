namespace Tourism_project.Models
{
    public class TourismTypeLocation
    {

        public int TourismTypeId { get; set; }
        public TourismType TourismType { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }

    }
}
