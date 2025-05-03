using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tourism_project.Dtos.Authentication
{
    public class TourismLogin
    {
        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username should be between 3 and 50 characters.")]

        public int UserName { get; set; }

        [Required]
        [PasswordPropertyText]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password should be at least 6 characters.")]

        public int Password { get; set; }
    }
}
