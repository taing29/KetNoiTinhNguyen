using TinhNguyenXanh.Models;
namespace TinhNguyenXanh.Interfaces
{
    public interface IOrganizationRepository
    {
        Task<IEnumerable<Organization>> GetAllAsync();
        Task<Organization?> GetByIdAsync(int id);

        // 🔹 Thêm mới:
        Task AddAsync(Organization organization);
        Task<Organization?> GetByUserIdAsync(string userId);
        Task SaveChangesAsync();
    }
}
