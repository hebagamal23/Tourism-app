namespace Tourism_project.Dtos.Home
{
    public class ActivityBookingRequestDTO
    {
        public int ActivityId { get; set; } 
        public string ActivityName { get; set; } 
        public decimal ActivityPrice { get; set; } 
        public int NumberOfGuests { get; set; } 
        public DateTime StartDate { get; set; } 

    
    }
}
