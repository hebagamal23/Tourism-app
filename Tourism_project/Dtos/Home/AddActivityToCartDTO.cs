namespace Tourism_project.Dtos.Home
{
    public class AddActivityToCartDTO
    {
        public int UserId { get; set; }
        public int ActivityId { get; set; }
        public int NumberOfGuests { get; set; }
        //public string ActivityName { get; set; }
        //public decimal ActivityPrice { get; set; }
        //public string ActivityImageUrl { get; set; }
        //public string LocationName { get; set; }
    }
}
