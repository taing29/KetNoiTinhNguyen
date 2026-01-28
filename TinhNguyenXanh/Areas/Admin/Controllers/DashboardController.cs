using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TinhNguyenXanh.Areas.Admin.Models;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Repositories;
using TinhNguyenXanh.Areas.Admin.Models.DTO;

namespace TinhNguyenXanh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class DashboardController : Controller
    {
        private readonly IStatisticRepository _statRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            IStatisticRepository statRepo,
            UserManager<ApplicationUser> userManager)
        {
            _statRepo = statRepo;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel
            {
                TotalEvents = await _statRepo.GetTotalEventsAsync(),
                TotalVolunteers = await _statRepo.GetTotalVolunteersAsync(),
                TotalOrganizations = await _statRepo.GetTotalOrganizationsAsync(),
                PendingReports = await _statRepo.GetPendingReportsCountAsync(),
                Top5FavoriteEvents = (await _statRepo.GetTopFavoriteEventsAsync(5))
                    .Select(e => new EventFavoriteDto
                    {
                        EventId = e.EventId,
                        Title = e.Title,
                        FavoriteCount = e.FavoriteCount
                    }).ToList(),

                MonthlyEventData = await _statRepo.GetMonthlyEventStatsAsync()
            };

            return View(vm);
        }
    }
}
