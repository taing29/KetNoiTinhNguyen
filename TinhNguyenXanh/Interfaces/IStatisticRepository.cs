using System.Collections.Generic;
using System.Threading.Tasks;
using TinhNguyenXanh.Areas.Admin.Models;
using TinhNguyenXanh.Areas.Admin.Models.DTO;
using TinhNguyenXanh.Models;
namespace TinhNguyenXanh.Interfaces
{
    public interface IStatisticRepository
    {
        Task<int> GetTotalEventsAsync();
        Task<int> GetTotalVolunteersAsync();
        Task<int> GetTotalOrganizationsAsync();
        Task<int> GetPendingReportsCountAsync();
        Task<IEnumerable<TopEventStatistic>> GetTopFavoriteEventsAsync(int count = 5);
        Task<List<MonthlyStatDto>> GetMonthlyEventStatsAsync();

        public class TopEventStatistic
        {
            public int EventId { get; set; }
            public string Title { get; set; } = string.Empty;
            public int FavoriteCount { get; set; }
        }
    }
}
