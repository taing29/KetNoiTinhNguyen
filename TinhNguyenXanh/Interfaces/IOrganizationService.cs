using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Interfaces
{
    public interface IOrganizationService
    {
        Task<IEnumerable<OrganizationDTO>> GetAllAsync();
        Task<OrganizationDTO?> GetByIdAsync(int id);

        Task<bool> RegisterAsync(OrganizationDTO model, string userId);
        Task<OrganizationDTO?> GetByUserIdAsync(string userId);
        Task<bool> UpdateAsync(OrganizationDTO model, string userId, IFormFile? avatarFile, IFormFile? docFile);
        Task<bool> AddReviewAsync(int organizationId, string userId, int rating, string comment);
        Task<bool> HasUserReviewedAsync(int organizationId, string userId);
        // IOrganizationService.cs – SỬA DÒNG NÀY THÀNH:
    }
}
