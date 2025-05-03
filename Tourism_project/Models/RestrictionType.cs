using System.ComponentModel.DataAnnotations;

namespace Tourism_project.Models
{
    public class RestrictionType
    {

        [Key]
        public int RestrictionTypeId { get; set; }
        public string Name { get; set; }  // مثل: "No Smoking", "No Pets"

        // العلاقة مع HotelRestriction
        public ICollection<HotelRestriction> HotelRestrictions { get; set; }  // ملاحظة: الاسم "HotelRestrictions" مهم في العلاقة

    }
}
