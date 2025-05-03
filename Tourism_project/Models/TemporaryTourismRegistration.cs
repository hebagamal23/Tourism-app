namespace Tourism_project.Models
{
    public class TemporaryTourismRegistration
    {
        public int Id { get; set; } // معرّف فريد
        public string FullName { get; set; } // اسم المستخدم بالكامل
        public string Email { get; set; } // البريد الإلكتروني للمستخدم
        public int PassportNumber { get; set; } // رقم جواز السفر
        public string Password { get; set; } // كلمة المرور
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // تاريخ الإنشاء

        // العلاقة مع TemporaryOtp
        public ICollection<TemporaryOtp> Otps { get; set; } // الكيانات المرتبطة
    }
}
