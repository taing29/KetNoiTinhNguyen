using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Areas.Admin.Models;
using TinhNguyenXanh.Areas.Admin.Models.DTO;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Interfaces;
using static TinhNguyenXanh.Interfaces.IStatisticRepository;

namespace TinhNguyenXanh.Repositories
{
    public class StatisticRepository : IStatisticRepository
    {
        private readonly ApplicationDbContext _context;

        public StatisticRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalEventsAsync()
        {
            return await _context.Events.Where(e => e.Status == "Approved").CountAsync();
        }

        public async Task<int> GetTotalVolunteersAsync()
        {
            return await _context.Volunteers.Where(v => v.Availability == "Available").CountAsync();
        }

        // THÊM: Tổng số tổ chức (Organizer)
        public async Task<int> GetTotalOrganizationsAsync()
        {
            return await _context.Organizations.Where(o => o.IsApproved == true).CountAsync();
        }

        // THÊM: Tổng số báo cáo đang chờ xử lý
        public async Task<int> GetPendingReportsCountAsync()
        {
            return await _context.EventReports.CountAsync(r => r.Status == "Pending");
        }

        // ĐÃ CÓ: Top 5 hoạt động yêu thích (chỉ sửa lại DTO cho khớp ViewModel)
        public async Task<IEnumerable<TopEventStatistic>> GetTopFavoriteEventsAsync(int count = 5)
        {
            var topEvents = await _context.EventFavorites
                .GroupBy(ef => ef.EventId)
                .Select(g => new
                {
                    EventId = g.Key,
                    FavoriteCount = g.Count()
                })
                .OrderByDescending(x => x.FavoriteCount)
                .Take(count)
                .Join(_context.Events,
                    f => f.EventId,
                    e => e.Id,
                    (f, e) => new TopEventStatistic
                    {
                        EventId = e.Id,
                        Title = e.Title,
                        FavoriteCount = f.FavoriteCount
                    })
                .ToListAsync();

            if (!topEvents.Any())
            {
                return await _context.Events
                    .OrderByDescending(e => e.Id)
                    .Take(5)
                    .Select(e => new TopEventStatistic
                    {
                        EventId = e.Id,
                        Title = e.Title,
                        FavoriteCount = 0
                    })
                    .ToListAsync();
            }

            return topEvents;
        }

        public async Task<List<MonthlyStatDto>> GetMonthlyEventStatsAsync()
        {
            var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-11).Date;
            var now = DateTime.UtcNow.Date;

            // Cách 1: Dùng client evaluation (an toàn, nhanh, không lỗi)
            var eventsInRange = await _context.Events
                .Where(e => e.StartTime >= twelveMonthsAgo && e.StartTime <= now)
                .ToListAsync();

            var result = eventsInRange
                .GroupBy(e => new { e.StartTime.Year, e.StartTime.Month })
                .Select(g => new MonthlyStatDto
                {
                    Month = $"{g.Key.Month:00}/{g.Key.Year}",
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            // Nếu không có dữ liệu → tạo đủ 12 tháng
            if (!result.Any())
            {
                result = new List<MonthlyStatDto>();
                for (int i = 11; i >= 0; i--)
                {
                    var date = DateTime.UtcNow.AddMonths(-i);
                    result.Add(new MonthlyStatDto
                    {
                        Month = $"{date.Month:00}/{date.Year}",
                        Count = 0
                    });
                }
            }

            return result;
        }
    }
}
