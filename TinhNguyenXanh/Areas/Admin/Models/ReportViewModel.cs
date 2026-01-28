namespace TinhNguyenXanh.Areas.Admin.Models
{
    public class ReportViewModel
    {
        public string ReporterName { get; set; } = null!;
        public string ReporterEmail { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public DateTime ReportedAt { get; set; }
    }
}
