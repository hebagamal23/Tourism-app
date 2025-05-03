namespace Tourism_project.Models
{
    public class MediaHotels
    {
        public int MediaId { get; set; }   
        public int HotelId { get; set; }  
        public string MediaType { get; set; }  
        public string MediaUrl { get; set; }

        public Hotel Hotel { get; set; }
    }
}
