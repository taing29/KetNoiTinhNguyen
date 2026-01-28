using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Repositories
{
    public class EventCategoryRepository : IEventCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public EventCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EventCategory>> GetAllCategoriesAsync()
        {
            return await _context.EventCategories.ToListAsync();
        }

        public async Task<EventCategory> GetCategoryByIdAsync(int id)
        {
            return await _context.EventCategories.FindAsync(id);
        }

        public async Task AddCategoryAsync(EventCategory category)
        {
            _context.EventCategories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(EventCategory category)
        {
            _context.EventCategories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.EventCategories.FindAsync(id);
            if (category != null)
            {
                _context.EventCategories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CategoryExists(int id)
        {
            return await _context.EventCategories.AnyAsync(e => e.Id == id);
        }
        public async Task<bool> CategoryHasEventsAsync(int categoryId)
        {
            return await _context.Events.AnyAsync(e => e.CategoryId == categoryId);
        }
    }
}
