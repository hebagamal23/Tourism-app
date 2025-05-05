namespace Tourism_project.Dtos.Home
{
    public class AddToCartDto
    {
        public int UserId { get; set; }
        public int ActivityId { get; set; }
        public string ActivityName { get; set; }
        public decimal ActivityPrice { get; set; }
        public string ActivityImageUrl { get; set; }
        public int LocationId { get; set; }
    }
}
