using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly ApplicationDbContext _context;

        public OrganizationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Organization>> GetAllAsync()
        {
            return await _context.Organizations
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task<Organization?> GetByIdAsync(int id)
        {
            return await _context.Organizations
                .Include(o => o.Reviews!)
                    .ThenInclude(r => r.User)
                .Include(o => o.User)
                .Include(o => o.Events) // 🆕 load luôn các sự kiện
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Organization?> GetByUserIdAsync(string userId)
        {
            return await _context.Organizations.FirstOrDefaultAsync(o => o.UserId == userId);
        }

        public async Task AddAsync(Organization organization)
        {
            await _context.Organizations.AddAsync(organization);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
