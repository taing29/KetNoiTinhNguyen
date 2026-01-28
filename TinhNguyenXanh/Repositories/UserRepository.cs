using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Interfaces;

namespace TinhNguyenXanh.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            // Lấy tất cả người dùng, không bao gồm Admin (tùy chọn)
            return await _context.Users.ToListAsync();
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<bool> LockUserAsync(string userId, DateTime? endDate = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Nếu không có endDate, khóa vĩnh viễn (đặt thời điểm rất xa)
            var lockoutEnd = endDate.HasValue ? endDate.Value : DateTimeOffset.MaxValue;

            var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
            return result.Succeeded;
        }

        public async Task<bool> UnlockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Đặt thời điểm khóa là NULL hoặc đã qua (ngay lập tức)
            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            return result.Succeeded;
        }
    }
}
