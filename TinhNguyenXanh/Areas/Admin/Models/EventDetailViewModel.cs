namespace TinhNguyenXanh.Areas.Admin.Models
{
    public class EventDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime StartTime { get; set; }
        public string Location { get; set; } = null!;
        public string OrganizerName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public bool IsHidden { get; set; }
        public string? HiddenReason { get; set; }
        public DateTime? HiddenAt { get; set; }
        public List<ReportViewModel> Reports { get; set; } = new();
    }
}
