// Areas/Admin/Controllers/AdminOrganizationsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;
using TinhNguyenXanh.Services;

namespace TinhNguyenXanh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminOrganizationsController : Controller
    {
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationRepository _orgRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminOrganizationsController(
            IOrganizationService organizationService,
            IOrganizationRepository orgRepo,
            UserManager<ApplicationUser> userManager)
        {
            _organizationService = organizationService;
            _orgRepo = orgRepo;
            _userManager = userManager;
        }

        // GET: /Admin/AdminOrganizations
        public async Task<IActionResult> Index(string search = "", int page = 1)
        {
            const int pageSize = 10;

            // Lấy tất cả tổ chức từ service (đảm bảo fresh data)
            var allOrgs = await _organizationService.GetAllAsync();

            // Chỉ hiển thị những tổ chức CHƯA được duyệt (IsApproved == false)
            var pendingOrgs = allOrgs.Where(o => !o.IsApproved).ToList();  // ToList() để force execute query

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                pendingOrgs = pendingOrgs.Where(o =>
                    o.Name.ToLower().Contains(search) ||
                    o.ContactEmail.ToLower().Contains(search) ||
                    (o.PhoneNumber?.ToLower().Contains(search) ?? false)).ToList();
            }

            var total = pendingOrgs.Count;

            var pagedOrgs = pendingOrgs
                .OrderByDescending(o => o.JoinedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling(total / (double)pageSize);


            return View(pagedOrgs);
        }

        // POST: Duyệt / Hủy duyệt + xóa role cũ trước khi gán mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleApproval(int id)
        {
            var org = await _orgRepo.GetByIdAsync(id);
            if (org == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tổ chức.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(org.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản người dùng liên kết.";
                return RedirectToAction(nameof(Index));
            }

            bool willApprove = !org.IsApproved;

            // Xóa tất cả role hiện tại trước
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            if (willApprove)
            {
                // DUYỆT: Gán role Organizer
                org.IsApproved = true;
                var addResult = await _userManager.AddToRoleAsync(user, SD.Role_Organizer);
                if (!addResult.Succeeded)
                {
                    TempData["ErrorMessage"] = "Duyệt thành công nhưng không thể cấp quyền Organizer.";
                }
                TempData["SuccessMessage"] = $"Đã duyệt tổ chức \"{org.Name}\" và cấp quyền Organizer thành công!";
            }
            else
            {
                // HỦY DUYỆT: Gán role mặc định (ví dụ "User" - thay nếu cần)
                org.IsApproved = false;
                await _userManager.AddToRoleAsync(user, "User");  // Hoặc SD.Role_User
                TempData["SuccessMessage"] = $"Đã hủy duyệt tổ chức \"{org.Name}\" và thu hồi quyền.";
            }

            await _orgRepo.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}