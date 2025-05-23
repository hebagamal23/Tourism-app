namespace Tourism_project.Models
{
    public class TemporaryTourismRegistration
    {
        public int Id { get; set; } 
        public string FullName { get; set; } 
        public string Email { get; set; } 
        public int PassportNumber { get; set; }
        public string Password { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

     
        public ICollection<TemporaryOtp> Otps { get; set; } 
    }
}
