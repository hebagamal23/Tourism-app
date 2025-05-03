using System.ComponentModel.DataAnnotations;

namespace Tourism_project.Dtos.Authentication
{
    public class ResetPasswordDto
    {


        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
