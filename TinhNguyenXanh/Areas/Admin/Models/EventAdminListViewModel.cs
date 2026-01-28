namespace TinhNguyenXanh.Areas.Admin.Models
{
    public class EventAdminListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string OrganizerName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public bool IsHidden { get; set; }
        public int ReportCount { get; set; }
        public string? ImageUrl { get; set; }
    }
}
