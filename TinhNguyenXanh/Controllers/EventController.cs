using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;
using TinhNguyenXanh.Models.ViewModel;

namespace TinhNguyenXanh.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _service;
        private readonly IEventRegistrationService _regService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public EventController(
            IEventService service,
            IEventRegistrationService regService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _regService = regService ?? throw new ArgumentNullException(nameof(regService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        // [1] Volunteer dashboard
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await GetOrCreateVolunteerAsync(userId);

            var stats = new VolunteerDashboardDTO
            {
                TotalEvents = await _context.EventRegistrations.CountAsync(r => r.VolunteerId == volunteer.Id),
                CompletedEvents = await _context.EventRegistrations.CountAsync(r => r.VolunteerId == volunteer.Id && r.Status == "Confirmed"),
                PendingEvents = await _context.EventRegistrations.CountAsync(r => r.VolunteerId == volunteer.Id && r.Status == "Pending"),
                TotalHours = await _context.EventRegistrations
                    .Where(r => r.VolunteerId == volunteer.Id && r.Status == "Confirmed")
                    .Join(_context.Events, reg => reg.EventId, evt => evt.Id, (reg, evt) =>
                        EF.Functions.DateDiffHour(evt.StartTime, evt.EndTime))
                    .SumAsync()
            };

            return View(stats);
        }

        // [2] Public events list (supports layout search redirect)
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? keyword = "", int? category = null, string? location = "", int page = 1, int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 5;

            // Base query: chỉ lấy event đã được duyệt
            var eventsQuery = _context.Events
                .AsNoTracking()
                .Include(e => e.Category)
                .Include(e => e.Organization)
                .Where(e => e.Status == "approved" && e.IsHidden == false);

            // Filters
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim();
                eventsQuery = eventsQuery.Where(e => e.Title.Contains(k) || e.Description.Contains(k));
            }

            if (category.HasValue && category.Value > 0)
            {
                eventsQuery = eventsQuery.Where(e => e.CategoryId == category.Value);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                var loc = location.Trim();
                eventsQuery = eventsQuery.Where(e => e.Location.Contains(loc));
            }

            // Total count for pagination
            var totalCount = await eventsQuery.CountAsync();

            // Paging + ordering
            var eventsPage = await eventsQuery
                .OrderByDescending(e => e.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new TinhNguyenXanh.DTOs.EventDTO
                {
                    Id = e.Id,
                    Title = e.Title,
                    Images = e.Images,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Location = e.Location,
                    CategoryName = e.Category != null ? e.Category.Name : null,
                    OrganizationName = e.Organization != null ? e.Organization.Name : null,
                    MaxVolunteers = e.MaxVolunteers,
                    Status = e.Status,
                    Description = e.Description,
                    IsHidden = e.IsHidden
                    // map thêm trường khác nếu DTO có
                })
                .ToListAsync();

            // Load organizations for search redirect / sidebar if needed
            var orgQuery = _context.Organizations
                .AsNoTracking()
                .Where(o => o.IsActive && o.Verified);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim();
                orgQuery = orgQuery.Where(o => o.Name.Contains(k) || o.Description.Contains(k));
            }
            if (!string.IsNullOrWhiteSpace(location))
            {
                var loc = location.Trim();
                orgQuery = orgQuery.Where(o => o.Address.Contains(loc));
            }
            var organizations = await orgQuery
                .OrderBy(o => o.Name)
                .Take(20)
                .ToListAsync();

            // Latest events as "Tin tức" for sidebar (no new model)
            var news = await _context.Events
                .AsNoTracking()
                .Where(e => e.Status == "approved" && e.IsHidden == false)
                .OrderByDescending(e => e.StartTime)
                .Take(5)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.Images,
                    e.StartTime
                })
                .ToListAsync();
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                // Lấy danh sách ID các sự kiện user này đã thích
                var favoritedEventIds = await _context.EventFavorites
                    .Where(f => f.UserId == userId)
                    .Select(f => f.EventId)
                    .ToListAsync();

                // Cập nhật trạng thái IsFavorited cho danh sách hiển thị
                foreach (var evt in eventsPage)
                {
                    if (favoritedEventIds.Contains(evt.Id))
                    {
                        evt.IsFavorited = true;
                    }
                }
            }

            // Pass metadata to view via ViewBag
            ViewBag.News = news;
            ViewBag.Organizations = organizations;
            ViewBag.TotalCount = totalCount;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = category;
            ViewBag.Location = location;

            // If no filters and you still prefer to use service mapping, you can fallback here.
            // But this implementation always returns EventDTO list built from the query above.
            return View(eventsPage);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(int eventId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var existingFavorite = await _context.EventFavorites
                .FirstOrDefaultAsync(f => f.EventId == eventId && f.UserId == userId);

            if (existingFavorite != null)
            {
                // Nếu đã thích rồi -> Xóa (Bỏ thích)
                _context.EventFavorites.Remove(existingFavorite);
                await _context.SaveChangesAsync();
                return Ok(new { status = "removed", message = "Đã bỏ yêu thích" });
            }
            else
            {
                // Chưa thích -> Thêm mới
                var favorite = new EventFavorite
                {
                    EventId = eventId,
                    UserId = userId,
                    FavoriteDate = DateTime.UtcNow
                };
                _context.EventFavorites.Add(favorite);
                await _context.SaveChangesAsync();
                return Ok(new { status = "added", message = "Đã thêm vào yêu thích" });
            }
        }

        // 2. Thêm action MyFavorites (Xem danh sách đã thích)
        [Authorize]
        public async Task<IActionResult> MyFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favoriteEvents = await _context.EventFavorites
                .Include(f => f.Event) // Include bảng Event
                .Where(f => f.UserId == userId)
                .Select(f => new EventDTO // Map sang DTO
                {
                    Id = f.Event.Id,
                    Title = f.Event.Title,
                    Images = f.Event.Images,
                    StartTime = f.Event.StartTime,
                    Location = f.Event.Location,
                    Status = f.Event.Status,
                    IsFavorited = true // Chắc chắn là true vì đang ở trang yêu thích
                })
                .ToListAsync();

            return View(favoriteEvents);
        }


        // [3] Event details
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            // Lấy event (DTO) từ service
            var evt = await _service.GetEventByIdAsync(id);
            if (evt == null) return NotFound();

            ViewBag.Message = TempData["Message"];
            ViewBag.Error = TempData["Error"];

            // Trạng thái đăng ký của user (nếu đã đăng nhập)
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var volunteer = await GetOrCreateVolunteerAsync(userId);
                var reg = await _context.EventRegistrations
                    .FirstOrDefaultAsync(r => r.EventId == id && r.VolunteerId == volunteer.Id);
                ViewBag.RegistrationStatus = reg?.Status;
            }

            // ===== Tin tức liên quan từ Events (không tạo model mới) =====
            // Ưu tiên: cùng tổ chức; nếu thiếu thì bổ sung theo cùng danh mục
            // Lấy tối đa 5 mục liên quan

            // Lấy các event cùng tổ chức (không bao gồm event hiện tại)
            var relatedByOrg = await _context.Events
                .Where(e => e.Status == "approved"
                            && e.OrganizationId == evt.OrganizationId
                            && e.Id != id)
                .OrderByDescending(e => e.StartTime)
                .Take(5)
                .ToListAsync();

            // Nếu chưa đủ 5, bổ sung theo cùng danh mục (không lấy event của cùng tổ chức đã lấy)
            var needMore = 5 - relatedByOrg.Count;
            var relatedByCategory = new List<Event>();

            if (needMore > 0)
            {
                relatedByCategory = await _context.Events
                    .Where(e => e.Status == "approved"
                                && e.CategoryId == evt.CategoryId
                                && e.OrganizationId != evt.OrganizationId
                                && e.Id != id)
                    .OrderByDescending(e => e.StartTime)
                    .Take(needMore)
                    .ToListAsync();
            }

            // Gộp kết quả và truyền xuống ViewBag
            var related = relatedByOrg.Concat(relatedByCategory).ToList();
            ViewBag.RelatedNews = related;

            // Trả về view với DTO (evt)
            return View(evt);
        }




        // [4] Register (GET)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> RegisterEvent(int id)
        {
            var evt = await _service.GetEventByIdAsync(id);
            if (evt == null || evt.Status != "approved") return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await GetOrCreateVolunteerAsync(userId);

            var existing = await _context.EventRegistrations
                .AnyAsync(r => r.EventId == id && r.VolunteerId == volunteer.Id);
            if (existing)
            {
                TempData["Message"] = "Bạn đã đăng ký sự kiện này.";
                return RedirectToAction("Details", new { id });
            }

            var currentCount = await _context.EventRegistrations
                .CountAsync(r => r.EventId == id && (r.Status == "Pending" || r.Status == "Confirmed"));
            if (currentCount >= evt.MaxVolunteers)
            {
                TempData["Error"] = "Sự kiện đã đủ số lượng tình nguyện viên.";
                return RedirectToAction("Details", new { id });
            }

            var dto = new EventRegistrationDTO
            {
                EventId = evt.Id,
                EventTitle = evt.Title,
                StartTime = evt.StartTime,
                EndTime = evt.EndTime,
                Location = evt.Location,
                FullName = volunteer.FullName ?? "",
                Phone = volunteer.Phone ?? ""
            };

            return View(dto);
        }

        // [5] Register (POST)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterEvent(EventRegistrationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var revt = await _service.GetEventByIdAsync(dto.EventId);
                if (revt != null)
                {
                    dto.EventTitle = revt.Title;
                    dto.StartTime = revt.StartTime;
                    dto.EndTime = revt.EndTime;
                    dto.Location = revt.Location;
                }
                return View(dto);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await GetOrCreateVolunteerAsync(userId);

            volunteer.FullName = dto.FullName;
            volunteer.Phone = dto.Phone;

            var existing = await _context.EventRegistrations
                .AnyAsync(r => r.EventId == dto.EventId && r.VolunteerId == volunteer.Id);
            if (existing)
            {
                TempData["Message"] = "Bạn đã đăng ký sự kiện này.";
                return RedirectToAction("Details", new { id = dto.EventId });
            }

            var evt = await _context.Events.FindAsync(dto.EventId);
            if (evt == null || evt.Status != "approved")
            {
                TempData["Error"] = "Sự kiện không hợp lệ.";
                return RedirectToAction("Details", new { id = dto.EventId });
            }

            var currentCount = await _context.EventRegistrations
                .CountAsync(r => r.EventId == dto.EventId && (r.Status == "Pending" || r.Status == "Confirmed"));
            if (currentCount >= evt.MaxVolunteers)
            {
                TempData["Error"] = "Sự kiện đã đủ số lượng tình nguyện viên.";
                return RedirectToAction("Details", new { id = dto.EventId });
            }

            var registration = new EventRegistration
            {
                EventId = dto.EventId,
                VolunteerId = volunteer.Id,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Reason = dto.Reason,
                Status = "Pending",
                RegisteredDate = DateTime.UtcNow
            };

            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Đăng ký thành công! Chờ duyệt.";
            return RedirectToAction("Details", new { id = dto.EventId });
        }

        // [6] My registrations
        [Authorize]
        public async Task<IActionResult> MyRegistrations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await GetOrCreateVolunteerAsync(userId);
            var regs = await _regService.GetByVolunteerIdAsync(volunteer.Id);
            return View(regs);
        }

        // [7] Volunteer profile (GET)
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await GetOrCreateVolunteerAsync(userId);

            var dto = new VolunteerProfileDTO
            {
                FullName = volunteer.FullName,
                Phone = volunteer.Phone,
                Email = volunteer.Email ?? User?.Identity?.Name,
                Address = volunteer.Address,
                Skills = volunteer.Skills,
                Bio = volunteer.Bio,
                AvatarUrl = volunteer.AvatarUrl
            };

            return View(dto);
        }

        // [8] Volunteer profile (POST)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(VolunteerProfileDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await GetOrCreateVolunteerAsync(userId);

            volunteer.FullName = dto.FullName;
            volunteer.Phone = dto.Phone;
            volunteer.Email = dto.Email;
            volunteer.Address = dto.Address;
            volunteer.Skills = dto.Skills;
            volunteer.Bio = dto.Bio;

            // Keep avatar as is unless changed via UploadAvatar
            _context.Volunteers.Update(volunteer);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Profile");
        }

        // [9] Upload avatar (POST)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile? avatarFile)
        {
            if (avatarFile == null || avatarFile.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ảnh hợp lệ.";
                return RedirectToAction("Profile");
            }

            // Basic validation: size < 5MB, image types only
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
            {
                TempData["Error"] = "Định dạng ảnh không hỗ trợ. Vui lòng chọn JPG/PNG/WebP.";
                return RedirectToAction("Profile");
            }
            if (avatarFile.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "Ảnh quá lớn (>5MB). Vui lòng chọn ảnh nhỏ hơn.";
                return RedirectToAction("Profile");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var volunteer = await GetOrCreateVolunteerAsync(userId);
            // **Lấy đối tượng ApplicationUser**
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // Xử lý nếu không tìm thấy User (trường hợp hiếm nếu đã Authorize)
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Profile");
            }
            // Ensure folder exists: wwwroot/images/avatars
            var folder = Path.Combine(_env.WebRootPath, "images", "avatars");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            volunteer.AvatarUrl = $"/images/avatars/{fileName}";
            
            _context.Volunteers.Update(volunteer);
            // **Cập nhật đường dẫn cho ApplicationUser**
            user.AvatarPath = $"/images/avatars/{fileName}";
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                // Xử lý lỗi nếu không thể cập nhật User
                TempData["Error"] = "Lỗi khi cập nhật thông tin người dùng.";
                return RedirectToAction("Profile");
            }
            await _context.SaveChangesAsync();

            TempData["Message"] = "Cập nhật ảnh đại diện thành công!";
            return RedirectToAction("Profile");
        }

        [AllowAnonymous]
        public async Task<IActionResult> GetComments(int eventId)
        {
            var comments = await _context.EventComments
                .Include(c => c.User)
                .Where(c => c.EventId == eventId && c.IsVisible && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return PartialView("_EventComments", comments);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostComment([FromForm] EventCommentDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content) || dto.Content.Length > 1000)
                return BadRequest("Nội dung không hợp lệ.");

            var evt = await _context.Events.FindAsync(dto.EventId);
            if (evt == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comment = new EventComment
            {
                EventId = dto.EventId,
                UserId = userId,
                Content = dto.Content.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsVisible = true,
                IsDeleted = false
            };

            _context.EventComments.Add(comment);
            await _context.SaveChangesAsync();

            var created = await _context.EventComments.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == comment.Id);
            return PartialView("_SingleComment", created);
        }


        // [C] Xóa hoặc ẩn comment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.EventComments.Include(c => c.Event).FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(currentUser);

            // Cho phép xóa nếu là admin hoặc chủ tổ chức của event hoặc tác giả comment
            var isAdmin = roles.Contains("Admin");
            var isOrganizer = roles.Contains("Organizer") && comment.Event != null && comment.Event.OrganizationId == /* tổ chức id của user nếu có */ 0;
            var isAuthor = comment.UserId == userId;

            if (!isAdmin && !isAuthor && !isOrganizer)
            {
                return Forbid();
            }

            comment.IsDeleted = true;
            comment.IsVisible = false;
            _context.EventComments.Update(comment);
            await _context.SaveChangesAsync();

            return Ok();
        }


        // === Helper: ensure volunteer record exists ===
        private async Task<Volunteer> GetOrCreateVolunteerAsync(string userId)
        {
            var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.UserId == userId);
            if (volunteer != null) return volunteer;

            var user = await _userManager.FindByIdAsync(userId);
            volunteer = new Volunteer
            {
                UserId = userId,
                FullName = user?.FullName ?? user?.UserName ?? "Tình nguyện viên",
                Email = user?.Email,
                Phone = user?.PhoneNumber,
                JoinedDate = DateTime.UtcNow,
                Availability = "Available"
            };

            _context.Volunteers.Add(volunteer);
            await _context.SaveChangesAsync();
            return volunteer;
        }
        // --- BỔ SUNG VÀO EventController.cs ---

        [HttpPost]
        [Authorize] // Bắt buộc phải đăng nhập mới được báo cáo
        public async Task<IActionResult> Report(int eventId, string reason)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                if (string.IsNullOrWhiteSpace(reason))
                {
                    return BadRequest(new { message = "Vui lòng nhập lý do báo cáo." });
                }

                // 1. Kiểm tra xem sự kiện có tồn tại không
                var eventExists = await _context.Events.AnyAsync(e => e.Id == eventId);
                if (!eventExists)
                {
                    return NotFound(new { message = "Sự kiện không tồn tại." });
                }

                // 2. (Tuỳ chọn) Kiểm tra xem user này đã báo cáo sự kiện này chưa?
                // Tránh spam báo cáo liên tục
                var existingReport = await _context.EventReports
                    .FirstOrDefaultAsync(r => r.EventId == eventId && r.ReporterUserId == userId);

                if (existingReport != null)
                {
                    return BadRequest(new { message = "Bạn đã gửi báo cáo cho sự kiện này rồi. Ban quản trị đang xem xét." });
                }

                // 3. Tạo đối tượng báo cáo mới
                var report = new EventReport
                {
                    EventId = eventId,
                    ReporterUserId = userId,
                    ReportReason = reason.Trim(),
                    ReportDate = DateTime.UtcNow,
                    Status = "Pending" // Trạng thái mặc định là Chờ xử lý
                };

                _context.EventReports.Add(report);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Gửi báo cáo thành công! Cảm ơn bạn đã đóng góp." });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                return StatusCode(500, new { message = "Có lỗi xảy ra, vui lòng thử lại sau." });
            }
        }

    }
}
