using Microsoft.AspNetCore.Mvc.Formatters;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tourism_project.Models
{
    public class RoomMedia
    {
        public int MediaId { get; set; } // المفتاح الأساسي
        [ForeignKey("Room")]
        public int RoomId { get; set; } // العلاقة مع الغرفة
        public Room Room { get; set; }

        [Required]
        public string MediaType { get; set; } // نوع الوسائط (صورة أو فيديو)

        [Required]
        [StringLength(255)]
        public string MediaUrl { get; set; } // رابط الصورة أو الفيديو

        public DateTime? UploadedAt { get; set; } = DateTime.Now; // وقت التحميل

    }
   
}
