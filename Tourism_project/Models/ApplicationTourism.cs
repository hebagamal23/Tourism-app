using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Tourism_project.Models
{
    public class ApplicationTourism : IdentityUser
    {
        //public int TouristId { get; set; }
        public string? StoredOTP { get; set; }
        public DateTime? OTPCreationTime { get; set; }

    }
}
