using Microsoft.AspNetCore.Mvc.Formatters;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tourism_project.Models
{
    public class RoomMedia
    {
        public int MediaId { get; set; } 
        [ForeignKey("Room")]
        public int RoomId { get; set; } 
        public Room Room { get; set; }

        [Required]
        public string MediaType { get; set; } 

        [Required]
        [StringLength(255)]
        public string MediaUrl { get; set; } 

        public DateTime? UploadedAt { get; set; } = DateTime.Now; 

    }
   
}
