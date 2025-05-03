//namespace Tourism_project.Models
//{
//    public class Transportation
//    {

//        public int Id { get; set; }
//        public string Name { get; set; } // اسم وسيلة النقل
//        public string Type { get; set; } // نوع وسيلة النقل (مثل طائرة، باص)
//        public int FromLocationId { get; set; } // المعرف الخاص بالموقع المنطلق
//        public int ToLocationId { get; set; } // المعرف الخاص بالموقع المقصد
//        public DateTime DepartureTime { get; set; } // وقت المغادرة
//        public DateTime ArrivalTime { get; set; } // وقت الوصول
//        public float Price { get; set; } // السعر
//        public int Capacity { get; set; } // السعة (عدد الركاب)

//        // العلاقة many-to-many مع Location عبر جدول TransportationLocation
//        public ICollection<TransportationLocation> TransportationLocations { get; set; }
//        // Enum لتحديد أنواع المواصلات (حافلة، سيارة، طائرة)
      
//    }
    

//}
