using System.ComponentModel.DataAnnotations;

namespace Tourism_project.Dtos.Authentication
{
    public class TourismRegistration
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Full Name cannot be longer than 100 characters.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Passport Number is required.")]
        [Range(10000000, 99999999, ErrorMessage = "Passport Number must be a valid 8-digit number.")]
        public int PassPortNumber { get ; set ; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        public string PassWord { get; set; }
    }
}
