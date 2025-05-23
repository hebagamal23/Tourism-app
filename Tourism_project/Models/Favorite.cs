using System.Diagnostics;

namespace Tourism_project.Models
{
    public class Favorite
    {
        public int FavoriteId { get; set; } 
        public int UserId { get; set; }

        public int ItemId { get; set; } 
        public string ItemType { get; set; } 

        public DateTime AddedAt { get; set; } = DateTime.Now; 

       
        public Hotel? Hotel { get; set; }
        public ACtivity? Activity { get; set; }
        public Location? Location { get; set; }
        public TourismType? TourismType { get; set; }

        public Tourism User { get; set; } 
    }
}
