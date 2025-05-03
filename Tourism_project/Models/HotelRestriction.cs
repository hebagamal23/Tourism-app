using System.ComponentModel.DataAnnotations;

namespace Tourism_project.Models
{
    public class HotelRestriction
    {
        [Key]
        public int RestrictionId { get; set; }

        public int HotelId { get; set; }  // المفتاح الأجنبي مع الجدول Hotel
        public Hotel Hotel { get; set; }

        public int RestrictionTypeId { get; set; }  // المفتاح الأجنبي مع جدول RestrictionType
        public RestrictionType RestrictionType { get; set; }

        public string Description { get; set; }  // وصف الحظر (اختياري)
    }
}
