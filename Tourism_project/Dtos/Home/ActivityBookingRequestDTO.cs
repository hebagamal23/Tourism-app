namespace Tourism_project.Dtos.Home
{
    public class ActivityBookingRequestDTO
    {
        public int ActivityId { get; set; } // معرّف النشاط
        public string ActivityName { get; set; } // اسم النشاط
        public decimal ActivityPrice { get; set; } // سعر النشاط
        public int NumberOfGuests { get; set; } // عدد الضيوف
        public DateTime StartDate { get; set; } // تاريخ البدء

       // public DateTime EndDate { get; set; }
    }
}
