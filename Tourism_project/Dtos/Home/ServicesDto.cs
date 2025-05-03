namespace Tourism_project.Dtos.Home
{
    public class ServicesDto
    {
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public string? IconName { get; set; }     // اسم الأيقونة (مثلاً: "wifi")

        public string? IconType { get; set; }     // نوع الأيقونة (مثلاً: "material" أو "fontawesome")


    }
}
