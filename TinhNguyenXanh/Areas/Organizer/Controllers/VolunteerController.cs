// Areas/Organizer/Controllers/VolunteersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Areas.Organizer.Controllers
{
    [Area("Organizer")]
    [Authorize(Roles = "Organizer")]
    public class VolunteersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VolunteersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? eventId, string search, int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organization == null)
                return RedirectToAction("Register", "Organization");

            var events = await _context.Events
                .Where(e => e.OrganizationId == organization.Id)
                .Select(e => new { e.Id, e.Title })
                .ToListAsync();

            ViewBag.EventList = new SelectList(events, "Id", "Title", eventId);
            ViewBag.Search = search;

            var query = _context.EventRegistrations
                .Include(r => r.Volunteer)
                .Include(r => r.Event)
                .Where(r => r.Event.OrganizationId == organization.Id && r.Status == "Pending");

            if (eventId.HasValue)
                query = query.Where(r => r.EventId == eventId.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(r => r.FullName.Contains(search) || r.Phone.Contains(search));

            int pageSize = 9;
            var total = await query.CountAsync();
            var registrations = await query
                .OrderByDescending(r => r.RegisteredDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalItems = total;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.CurrentPage = page;

            return View(registrations);
        }

        // 🆕 THÊM ACTION MỚI: Xem danh sách tình nguyện viên theo sự kiện
        public async Task<IActionResult> ListVolunteer(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organization == null)
                return RedirectToAction("Register", "Organization");

            // Kiểm tra sự kiện có thuộc tổ chức không
            var evt = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id && e.OrganizationId == organization.Id);

            if (evt == null)
            {
                TempData["Error"] = "Không tìm thấy sự kiện hoặc bạn không có quyền truy cập";
                return RedirectToAction("Index", "Event");
            }

            // Lấy danh sách đăng ký (tất cả trạng thái)
            var registrations = await _context.EventRegistrations
                .Include(r => r.Volunteer)
                    .ThenInclude(v => v.User)
                .Where(r => r.EventId == id)
                .OrderByDescending(r => r.RegisteredDate)
                .ToListAsync();

            ViewBag.EventTitle = evt.Title;
            ViewBag.EventId = evt.Id;

            return View(registrations);
        }

        // 🆕 Xem chi tiết tình nguyện viên
        public async Task<IActionResult> VolunteerDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organization == null)
                return RedirectToAction("Register", "Organization");

            var registration = await _context.EventRegistrations
                .Include(r => r.Volunteer)
                    .ThenInclude(v => v.User)
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.Id == id && r.Event.OrganizationId == organization.Id);

            if (registration == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin tình nguyện viên.";
                return RedirectToAction("Index");
            }

            return View(registration);
        }
        // GET: Organizer/Volunteers/History/5
        public async Task<IActionResult> History(int volunteerId)
        {
            // 1. Xác định Tổ chức đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organization == null) return RedirectToAction("Register", "Organization");

            // 2. Lấy thông tin Tình nguyện viên
            var volunteer = await _context.Volunteers
                .Include(v => v.User) // Lấy thêm info User để hiển thị Email, Avatar nếu cần
                .FirstOrDefaultAsync(v => v.Id == volunteerId);

            if (volunteer == null) return NotFound();

            // 3. Lấy danh sách các sự kiện (Của tổ chức này) mà TVN đó đã đăng ký
            var history = await _context.EventRegistrations
                .Include(r => r.Event)
                .Where(r => r.VolunteerId == volunteerId && r.Event.OrganizationId == organization.Id)
                .OrderByDescending(r => r.RegisteredDate)
                .ToListAsync();

            // 4. Truyền thông tin cá nhân qua ViewBag để hiển thị ở Header
            ViewBag.VolunteerInfo = volunteer;

            return View(history);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var reg = await _context.EventRegistrations
                .Include(r => r.Event)
                    .ThenInclude(e => e.Organization)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reg == null || reg.Status != "Pending" || reg.Event.Organization?.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "Không thể duyệt.";
                return RedirectToAction(nameof(Index));
            }

            reg.Status = "Confirmed";
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã duyệt {reg.FullName}";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var reg = await _context.EventRegistrations
                .Include(r => r.Event)
                    .ThenInclude(e => e.Organization)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reg == null || reg.Status != "Pending" || reg.Event.Organization?.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "Không thể từ chối.";
                return RedirectToAction(nameof(Index));
            }

            reg.Status = "Rejected";
            await _context.SaveChangesAsync();

            TempData["Error"] = $"Đã từ chối {reg.FullName}";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Certificates()
        {
            return View();
        }
    }
}