using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Tourism_project.Models
{
    public class Service
    {

        public int ServiceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public int DurationHours { get; set; }

        public string? IconName { get; set; }     

        public string? IconType { get; set; }     

        public ICollection<HotelService> HotelServices { get; set; }
        
    public ICollection<RoomService> RoomServices { get; set; }
    }
}
