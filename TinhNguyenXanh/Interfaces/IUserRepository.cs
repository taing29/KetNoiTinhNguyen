using TinhNguyenXanh.Data;

namespace TinhNguyenXanh.Interfaces
{
    public interface IUserRepository
    {
        // Lấy danh sách tài khoản (có thể lọc theo vai trò Admin, Volunteer, Organization, User)
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();

        // Khóa tài khoản vĩnh viễn hoặc đến một thời điểm cụ thể
        Task<bool> LockUserAsync(string userId, DateTime? endDate = null);

        // Mở khóa tài khoản
        Task<bool> UnlockUserAsync(string userId);

        // Lấy thông tin người dùng theo Id
        Task<ApplicationUser> GetUserByIdAsync(string userId);
    }
}
