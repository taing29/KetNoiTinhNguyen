using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Controllers
{
    public class OrganizationController : Controller
    {
        private readonly IOrganizationService _organizationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public OrganizationController(
            IOrganizationService organizationService,
            UserManager<ApplicationUser> userManager, 
            ApplicationDbContext context)
        {
            _organizationService = organizationService;
            _userManager = userManager;
            _context = context;
        }

        // GET: /Organization
        // GET: /Organization
        public async Task<IActionResult> Index(string? keyword = "", int page = 1, int pageSize = 5) // Mặc định 5 tổ chức/trang
        {
            if (page < 1) page = 1;

            // 1. Lấy tất cả dữ liệu
            var allOrganizations = await _organizationService.GetAllAsync();

            // 2. Lọc theo từ khóa (nếu có)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim().ToLower();
                allOrganizations = allOrganizations.Where(o =>
                    o.Name.ToLower().Contains(k) ||
                    (o.Description != null && o.Description.ToLower().Contains(k))
                );
            }

            // 3. Tính toán phân trang
            var totalCount = allOrganizations.Count();

            var pagedOrganizations = allOrganizations
                .OrderByDescending(o => o.JoinedDate) // Sắp xếp mới nhất lên đầu (tùy chọn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 4. Truyền thông tin sang View
            ViewBag.TotalCount = totalCount;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Keyword = keyword;

            // --- PHẦN 2: LẤY TIN TỨC (THÊM MỚI ĐOẠN NÀY) ---
            // Lấy 5 sự kiện mới nhất đã được duyệt để làm tin tức
            var news = await _context.Events
                .AsNoTracking()
                .Where(e => e.Status == "approved")
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

            ViewBag.News = news; // Truyền sang View

            return View(pagedOrganizations);
        }

        // GET: /Organization/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var organization = await _organizationService.GetByIdAsync(id);
            if (organization == null)
            {
                return NotFound();
            }
            return View(organization);
        }

        // GET: /Organization/Register
        [Authorize]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Organization/Register
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] OrganizationDTO model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var success = await _organizationService.RegisterAsync(model, user.Id);
                Console.WriteLine($"RegisterAsync trả về: {success}");

                if (success)
                {
                    TempData["SuccessMessage"] = "Đăng ký tổ chức thành công! Bạn đã trở thành Ban tổ chức.";
                    return RedirectToAction(nameof(Success));
                }

                ModelState.AddModelError("", "Đăng ký thất bại (không rõ lý do)");
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            // Debug ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine($"[ModelState Invalid] Errors: {string.Join(", ", errors)}");
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    Console.WriteLine("[Register] User not found from User.Identity");
                    return Unauthorized();
                }

                Console.WriteLine($"[Register] Starting registration for user: {user.Id}");
                Console.WriteLine($"[Register] Model - Name: {model.Name}, Description: {model.Description?.Length} chars");

                var success = await _organizationService.RegisterAsync(model, user.Id);

                Console.WriteLine($"[Register] RegisterAsync returned: {success}");

                if (success)
                {
                    TempData["SuccessMessage"] = "Đăng ký tổ chức thành công! Bạn đã trở thành Ban tổ chức.";
                    return RedirectToAction(nameof(Success));
                }

                // Trường hợp này không nên xảy ra nữa vì service luôn throw exception
                Console.WriteLine("[Register] WARNING: RegisterAsync returned false without throwing exception!");
                ModelState.AddModelError("", "Đăng ký tổ chức thất bại. Vui lòng thử lại.");
                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[Register] InvalidOperationException: {ex.Message}");
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"[Register] ArgumentException: {ex.Message}");
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Register] Unexpected Exception: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Lỗi hệ thống: {ex.Message}");
                return View(model);
            }
        }

        // GET: /Organization/Success
        public IActionResult Success()
        {
            return View();
        }

        // POST: /Organization/SubmitReview/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Volunteer")]
        public async Task<IActionResult> SubmitReview(int id, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Vui lòng chọn số sao hợp lệ.";
                return RedirectToAction("Details", new { id });
            }

            var userId = _userManager.GetUserId(User);

            // DÙNG SERVICE ĐÂY NÈ – ĐẸP, SẠCH, DỄ TEST!
            bool alreadyReviewed = await _organizationService.HasUserReviewedAsync(id, userId);
            if (alreadyReviewed)
            {
                TempData["Warning"] = "Bạn đã đánh giá tổ chức này rồi!";
                return RedirectToAction("Details", new { id });
            }

            bool success = await _organizationService.AddReviewAsync(id, userId, rating, comment);

            if (success)
            {
                TempData["Success"] = "Cảm ơn bạn! Đánh giá đã được ghi nhận.";
            }
            else
            {
                TempData["Error"] = "Không thể gửi đánh giá. Tổ chức có thể không tồn tại.";
            }

            return RedirectToAction("Details", new { id });
        }
    }
}