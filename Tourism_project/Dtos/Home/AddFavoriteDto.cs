namespace Tourism_project.Dtos.Home
{
    public class AddFavoriteDto
    {
        public int UserId { get; set; }
        public int ItemId { get; set; }
        public string ItemType { get; set; }

        public string? ImageUrl { get; set; }
    }
}
