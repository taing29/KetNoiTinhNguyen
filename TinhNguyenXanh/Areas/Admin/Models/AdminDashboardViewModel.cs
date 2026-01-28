using TinhNguyenXanh.Areas.Admin.Models.DTO;
using TinhNguyenXanh.Interfaces;

namespace TinhNguyenXanh.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int TotalVolunteers { get; set; }
        public int TotalOrganizations { get; set; }
        public int PendingReports { get; set; }
        public List<EventFavoriteDto> Top5FavoriteEvents { get; set; } = new();
        public List<MonthlyStatDto> MonthlyEventData { get; set; } = new();
    }

    //public class EventFavoriteDto
    //{
    //    public int EventId { get; set; }
    //    public string Title { get; set; } = string.Empty;
    //    public int FavoriteCount { get; set; }
    //}

    //public class MonthlyStatDto
    //{
    //    public string Month { get; set; } = string.Empty;
    //    public int Count { get; set; }
    //}
}
