
namespace Tourism_project.Dtos.Home
{
    public class RoomDto
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public int MaxOccupancy { get; set; }  
        public string RoomImageUrl { get; set; }  
        public string description { get; set; } 
        public int BedCount { get; set; }
        public double Size { get; set; } 
        public decimal PricePerNight { get; set; } 
        public bool IsAvailable { get; set; } = true; 
    }
}
