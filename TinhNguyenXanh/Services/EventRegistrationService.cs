using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Services
{
    public class EventRegistrationService : IEventRegistrationService
    {
        private readonly ApplicationDbContext _context;

        public EventRegistrationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegisterAsync(EventRegistrationDTO dto, string userId)
        {
            var volunteer = await _context.Volunteers
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (volunteer == null) return false;

            var existing = await _context.EventRegistrations
                .AnyAsync(r => r.EventId == dto.EventId && r.VolunteerId == volunteer.Id);

            if (existing) return false;

            var registration = new EventRegistration
            {
                EventId = dto.EventId,
                VolunteerId = volunteer.Id,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Reason = dto.Reason,
                Status = "Pending",
                RegisteredDate = DateTime.UtcNow
            };

            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<EventRegistration?> GetByIdAsync(int id)
        {
            return await _context.EventRegistrations
                .Include(r => r.Event)
                .Include(r => r.Volunteer)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<EventRegistration>> GetByEventIdAsync(int eventId)
        {
            return await _context.EventRegistrations
                .Include(r => r.Event)
                .Include(r => r.Volunteer)
                .Where(r => r.EventId == eventId)
                .ToListAsync();
        }

        public async Task<IEnumerable<EventRegistration>> GetByVolunteerIdAsync(int volunteerId)
        {
            return await _context.EventRegistrations
                .Include(r => r.Event)
                .Where(r => r.VolunteerId == volunteerId)
                .OrderByDescending(r => r.RegisteredDate)
                .ToListAsync();
        }

        public async Task<bool> ApproveAsync(int id)
        {
            var reg = await _context.EventRegistrations.FindAsync(id);
            if (reg == null || reg.Status != "Pending") return false;

            reg.Status = "Confirmed";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(int id)
        {
            var reg = await _context.EventRegistrations.FindAsync(id);
            if (reg == null || reg.Status != "Pending") return false;

            reg.Status = "Rejected";
            await _context.SaveChangesAsync();
            return true;
        }
    }
}