using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tourism_project.Models
{
    public class PaymentMethod
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentMethodId { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "Payment method name cannot exceed 50 characters.")]
        public string MethodName { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Transaction fee must be a positive value.")]
        public decimal TransactionFee { get; set; } = 0;

        public ICollection<Payment> Payments { get; set; }

    }
}
