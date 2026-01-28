using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Areas.Admin.Models;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public EventsController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index(string search = "", int page = 1)
        {
            int pageSize = 10;
            var query = _context.Events
                .Include(e => e.Category)
                .Include(e => e.Organization)
                .Include(e => e.Reports)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(e =>
                    e.Title.ToLower().Contains(s) ||
                    (e.Organization != null && e.Organization.Name.ToLower().Contains(s)));
            }

            var total = await query.CountAsync();
            var events = await query
                .OrderByDescending(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EventAdminListViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    OrganizerName = e.Organization != null ? e.Organization.Name : "Không xác định",
                    CategoryName = e.Category != null ? e.Category.Name : "Chưa có",
                    StartTime = e.StartTime,
                    IsHidden = e.IsHidden,
                    ReportCount = e.Reports.Count,
                    ImageUrl = e.Images,
                })
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            return View(events);
        }

        // AJAX: Trả JSON cho modal chi tiết
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Organization)
                .Include(e => e.Reports!).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return NotFound();

            var model = new
            {
                id = ev.Id,
                title = ev.Title,
                description = ev.Description,
                imageUrl = ev.Images?.Split(',')[0]?.Trim(),
                startTime = ev.StartTime,
                location = ev.Location,
                organizerName = ev.Organization?.Name ?? "Không xác định",
                categoryName = ev.Category?.Name ?? "Chưa có",
                isHidden = ev.IsHidden,
                hiddenReason = ev.HiddenReason,
                hiddenAt = ev.HiddenAt,
                reports = ev.Reports.Select(r => new
                {
                    reporterName = r.User.FullName ?? r.User.Email.Split('@')[0],
                    reporterEmail = r.User.Email,
                    reason = r.ReportReason,
                    reportedAt = r.ReportDate
                }).OrderByDescending(r => r.reportedAt).ToList()
            };

            return Json(model);
        }

        // Ẩn / Hiện (không cần lý do)
        [HttpPost]
        public async Task<IActionResult> ToggleHide(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return Json(new { success = false, message = "Không tìm thấy hoạt động!" });

            ev.IsHidden = !ev.IsHidden;

            if (ev.IsHidden)
            {
                ev.HiddenReason = "Ẩn bởi quản trị viên";
                ev.HiddenAt = DateTime.Now;
            }
            else
            {
                ev.HiddenReason = null;
                ev.HiddenAt = null;
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = ev.IsHidden ? "Đã ẩn hoạt động" : "Đã hiện lại hoạt động"
            });
        }

        // Xóa vĩnh viễn
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Reports)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return Json(new { success = false });

            _context.EventReports.RemoveRange(ev.Reports);
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa hoạt động và toàn bộ báo cáo!" });
        }
    }
}