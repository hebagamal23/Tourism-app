using System.ComponentModel.DataAnnotations;

namespace Tourism_project.Models
{
    public class RestrictionType
    {

        [Key]
        public int RestrictionTypeId { get; set; }
        public string Name { get; set; } 

        // العلاقة مع HotelRestriction
        public ICollection<HotelRestriction> HotelRestrictions { get; set; } 

    }
}
