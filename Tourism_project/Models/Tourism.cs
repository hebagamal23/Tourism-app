using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tourism_project.Models
{
    public class Tourism
    {
        [Key]
        public int TouristId { get; set; }

        [Required]
        public string AspNetUserId { get; set; } // المفتاح الخارجي الذي يربط بـ AspNetUsers

        [Required]
        public int PassportNumber { get; set; }

        public string? Poster { get; set; }

        // العلاقة مع جدول AspNetUsers
        [ForeignKey(nameof(AspNetUserId))]
        public ApplicationTourism AspNetUser { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        // علاقة متعددة إلى متعددة
        public ICollection<TouristTourismType> TouristTourismTypes { get; set; }


    }

}
