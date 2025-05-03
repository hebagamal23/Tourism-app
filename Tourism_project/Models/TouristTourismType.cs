namespace Tourism_project.Models
{
    public class TouristTourismType
    {

        public int TouristId { get; set; }
        public Tourism Tourist { get; set; }

        public int TourismTypeId { get; set; }
        public TourismType TourismType { get; set; }


    }
}
