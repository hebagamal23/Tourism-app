namespace Tourism_project.Models
{
    public class TemporaryOtp
    {
        public int Id { get; set; } 
        public string Email { get; set; } 
        public string OtpCode { get; set; } 
        public DateTime ExpiryDate { get; set; } 
        public bool IsVerified { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

       
        public int TemporaryUserId { get; set; } 
        public TemporaryTourismRegistration TemporaryUser { get; set; }
    }
}

