using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

[Area("Organizer")]
[Authorize(Roles = "Organizer")]
public class EventManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IEventService _eventService;

    public EventManagementController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IEventService eventService)
    {
        _userManager = userManager;
        _context = context;
        _eventService = eventService;
    }

    // GET: Index + Filter
    public async Task<IActionResult> Index(string? status)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.UserId == user.Id);

        if (organization == null)
            return RedirectToAction("Register", "Organization");

        var events = await _eventService.GetEventsByOrganizationAsync(organization.Id);

        // Áp dụng filter nếu có
        if (!string.IsNullOrEmpty(status))
        {
            events = events.Where(e =>
                e.Status?.Equals(status, StringComparison.OrdinalIgnoreCase) == true);
        }

        ViewBag.CurrentFilter = status;
        return View(events);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = new SelectList(await _context.EventCategories.ToListAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventDTO model)
    {
        ViewBag.Categories = new SelectList(await _context.EventCategories.ToListAsync(), "Id", "Name");

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            ModelState.AddModelError("", "Không tìm thấy người dùng.");
            return View(model);
        }

        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.UserId == user.Id);

        if (organization == null)
        {
            ModelState.AddModelError("", "Không tìm thấy tổ chức của bạn.");
            return View(model);
        }

        var result = await _eventService.CreateEventAsync(model, organization.Id);
        if (!result)
        {
            ModelState.AddModelError("", "Không thể tạo sự kiện. Vui lòng thử lại.");
            return View(model);
        }

        TempData["Success"] = "Tạo sự kiện thành công! Đang chờ duyệt.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.UserId == user.Id);
        if (organization == null)
            return RedirectToAction("Register", "Organization");

        var evt = await _eventService.GetEventByIdAsync(id);
        if (evt == null || evt.OrganizationId != organization.Id)
            return NotFound();

        return View(evt);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.UserId == user.Id);
        if (organization == null)
            return RedirectToAction("Register", "Organization");

        var evt = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == id && e.OrganizationId == organization.Id);
        if (evt == null) return NotFound();

        var model = new EventDTO
        {
            Id = evt.Id,
            Title = evt.Title,
            Description = evt.Description,
            Location = evt.Location,
            LocationCoords = evt.LocationCoords,
            StartTime = evt.StartTime,
            EndTime = evt.EndTime,
            MaxVolunteers = evt.MaxVolunteers,
            CategoryId = evt.CategoryId,
            Images = evt.Images,
            OrganizationId = organization.Id
        };

        ViewBag.Categories = new SelectList(
            await _context.EventCategories.ToListAsync(), "Id", "Name", evt.CategoryId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EventDTO model)
    {
        ViewBag.Categories = new SelectList(
            await _context.EventCategories.ToListAsync(), "Id", "Name", model.CategoryId);

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            ModelState.AddModelError("", "Không tìm thấy người dùng.");
            return View(model);
        }

        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.UserId == user.Id);
        if (organization == null)
        {
            ModelState.AddModelError("", "Không tìm thấy tổ chức.");
            return View(model);
        }

        var result = await _eventService.UpdateEventAsync(model, organization.Id);
        if (!result)
        {
            ModelState.AddModelError("", "Cập nhật sự kiện thất bại. Vui lòng thử lại.");
            return View(model);
        }

        TempData["Success"] = "Cập nhật sự kiện thành công!";
        return RedirectToAction(nameof(Index));
    }

    // POST: Ẩn sự kiện
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Hide(int id, string? status)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.UserId == user.Id);
        if (organization == null) return NotFound();

        var evt = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == id && e.OrganizationId == organization.Id);
        if (evt == null) return NotFound();

        if (!evt.Status.Equals("approved", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Sự kiện chưa được duyệt nên không thể ẩn.";
            return RedirectToAction(nameof(Index), new { status });
        }

        evt.Status = "hidden";
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Đã ẩn sự kiện \"{evt.Title}\".";
        return RedirectToAction(nameof(Index), new { status });
    }

    // POST: Hiện lại sự kiện
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unhide(int id, string? status)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.UserId == user.Id);
        if (organization == null) return NotFound();

        var evt = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == id && e.OrganizationId == organization.Id);
        if (evt == null) return NotFound();

        evt.Status = "approved"; // hoặc "draft"
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Đã gửi yêu cầu hiện lại sự kiện \"{evt.Title}\". Đang chờ duyệt.";
        return RedirectToAction(nameof(Index), new { status });
    }


    public IActionResult Registrations()
    {
        return RedirectToAction("Index", "Volunteers", new { area = "Organizer" });
    }
}