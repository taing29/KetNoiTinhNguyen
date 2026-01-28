using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Interfaces
{
    public interface IEventReportRepository
    {
        // Lấy tất cả báo cáo, có thể sắp xếp theo trạng thái (Pending, Resolved)
        Task<IEnumerable<EventReport>> GetAllReportsAsync();

        // Lấy chi tiết báo cáo
        Task<EventReport> GetReportByIdAsync(int reportId);

        // Cập nhật trạng thái xử lý báo cáo (Pending -> Resolved/Rejected)
        Task<bool> UpdateReportStatusAsync(int reportId, string newStatus);

        // Lấy tất cả báo cáo liên quan đến một sự kiện (Event)
        Task<IEnumerable<EventReport>> GetReportsByEventIdAsync(int eventId);

        // Xóa báo cáo sau khi xử lý (tùy chọn)
        Task<bool> DeleteReportAsync(int reportId);
    }
}
