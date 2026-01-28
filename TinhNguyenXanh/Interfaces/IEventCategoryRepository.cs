using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Interfaces
{
    public interface IEventCategoryRepository
    {
        Task<IEnumerable<EventCategory>> GetAllCategoriesAsync();
        Task<EventCategory> GetCategoryByIdAsync(int id);
        Task AddCategoryAsync(EventCategory category);
        Task UpdateCategoryAsync(EventCategory category);
        Task<bool> CategoryHasEventsAsync(int categoryId);
        Task DeleteCategoryAsync(int id);
        Task<bool> CategoryExists(int id);
    }
}
