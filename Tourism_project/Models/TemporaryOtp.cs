namespace Tourism_project.Models
{
    public class TemporaryOtp
    {
        public int Id { get; set; } // معرّف فريد
        public string Email { get; set; } // البريد الإلكتروني للمستخدم
        public string OtpCode { get; set; } // كود OTP
        public DateTime ExpiryDate { get; set; } // تاريخ انتهاء صلاحية الكود
        public bool IsVerified { get; set; } // حالة التحقق (صحيح أو خاطئ)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // تاريخ الإنشاء

        // العلاقة مع TemporaryTourismRegistration
        public int TemporaryUserId { get; set; } // المفتاح الأجنبي
        public TemporaryTourismRegistration TemporaryUser { get; set; } // الكيان المرتبط
    }
}

