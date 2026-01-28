using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Areas.Admin.Models;

namespace TinhNguyenXanh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string search = "", int page = 1)
        {
            int pageSize = 10;

            var adminUsers = await _userManager.GetUsersInRoleAsync(SD.Role_Admin);
            var adminIds = adminUsers.Select(u => u.Id).ToHashSet();

            var usersQuery = _userManager.Users
                .Where(u => !adminIds.Contains(u.Id));

            if (!string.IsNullOrEmpty(search))
            {
                var lower = search.Trim().ToLower();
                usersQuery = usersQuery.Where(u =>
                    u.Email.ToLower().Contains(lower) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(lower)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(search.Trim())));
            }

            var total = await usersQuery.CountAsync();

            var users = await usersQuery
                .OrderBy(u => u.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var isLocked = user.LockoutEnabled &&
                               user.LockoutEnd.HasValue &&
                               user.LockoutEnd.Value > DateTimeOffset.UtcNow;

                viewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName ?? "Chưa đặt tên",
                    PhoneNumber = user.PhoneNumber ?? "Chưa có",
                    IsLocked = isLocked,
                    Roles = roles
                });
            }

            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

            return View(viewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock([FromBody] string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { success = false, message = "ID không hợp lệ!" });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng!" });

            if (await _userManager.IsInRoleAsync(user, SD.Role_Admin))
                return Json(new { success = false, message = "Không thể khóa tài khoản Admin!" });

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
                await _userManager.SetLockoutEndDateAsync(user, null); 
            else
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(200));

            var status = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow ? "khóa" : "mở khóa";
            return Json(new { success = true, message = $"Đã {status} tài khoản thành công!" });
        }
    }
}