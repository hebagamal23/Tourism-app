namespace Tourism_project.Models
{
    public class Service
    {

        public int ServiceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public int DurationHours { get; set; }

        public string? IconName { get; set; }     // اسم الأيقونة (مثلاً: "wifi")

        public string? IconType { get; set; }     // نوع الأيقونة (مثلاً: "material" أو "fontawesome")

        public ICollection<HotelService> HotelServices { get; set; }
    }
}
