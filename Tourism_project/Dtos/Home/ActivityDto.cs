using Tourism_project.Models;

namespace Tourism_project.Dtos.Home
{
    public class ActivityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? MoreDescription { get; set; }

        public float Price { get; set; }
        public int DurationHours { get; set; }

        public string? ImageUrl { get; set; }
        public string LocationName { get; set; }
    }
}
