using System.ComponentModel.DataAnnotations;

namespace Tourism_project.Dtos.Authentication
{
    public class ForgetPasswordDto
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
