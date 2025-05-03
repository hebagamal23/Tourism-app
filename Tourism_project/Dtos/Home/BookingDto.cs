namespace Tourism_project.Dtos.Home
{
    public class BookingDto
    {

        public int RoomId { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
       
        public int PaymentMethodId { get; set; }
        public int NumberOfGuests { get; set; } // ✅ عدد الأفراد في الحجز



    }
}
