using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Repositories
{
    public class EventReportRepository : IEventReportRepository
    {
        private readonly ApplicationDbContext _context;

        public EventReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EventReport>> GetAllReportsAsync()
        {
            // Include Event và ReporterUser để hiển thị thông tin chi tiết
            return await _context.EventReports
                .Include(r => r.Event)
                .Include(r => r.User)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
        }

        public async Task<EventReport> GetReportByIdAsync(int reportId)
        {
            return await _context.EventReports
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reportId);
        }

        public async Task<bool> UpdateReportStatusAsync(int reportId, string newStatus)
        {
            var report = await _context.EventReports.FirstOrDefaultAsync(r => r.Id == reportId);
            if (report == null) return false;

            report.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<EventReport>> GetReportsByEventIdAsync(int eventId)
        {
            return await _context.EventReports
                .Where(r => r.EventId == eventId)
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<bool> DeleteReportAsync(int reportId)
        {
            var report = await _context.EventReports.FindAsync(reportId);
            if (report == null) return false;

            _context.EventReports.Remove(report);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
