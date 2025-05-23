namespace Tourism_project.Dtos.Home
{
    public class LocationDto
    {
        
            public int Id { get; set; }
            public string Name { get; set; }
            public string ImageUrl { get; set; }
            public string Description { get; set; }

        public tourismTypeIDName TourismType { get; set; }

    }
}
