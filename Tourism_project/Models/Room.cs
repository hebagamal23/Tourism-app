namespace Tourism_project.Models
{
    public class Room
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string description { get; set; }

        public RoomType Type { get; set; } 

        public int BedCount { get; set; }
        public int MaxOccupancy { get; set; } 
        public double Size { get; set; } 
        public decimal PricePerNight { get; set; } 
        public bool IsAvailable { get; set; } = true; 

        public int HotelId { get; set; } 
        public Hotel Hotel { get; set; }

       
        public ICollection<Booking> Bookings { get; set; } 

                                                         
        public List<RoomMedia> Media { get; set; }

     
        public ICollection<RoomService> RoomServices { get; set; }


        public enum RoomType
        {
            Single,
            Double,
            King_size_Bed,
            Suite
        }

    }
}
