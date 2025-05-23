namespace Tourism_project.Dtos.Home
{
    public class BookingDtoOutput
    {

        public int BookingId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfGuests { get; set; }
        public string TouristName { get; set; }  
        public string BookingStatus { get; set; }  
    }
}
