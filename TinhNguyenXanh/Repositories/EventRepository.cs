using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;

        public EventRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _context.Events
                .Include(e => e.Organization)
                .Include(e => e.Category)
                .ToListAsync();
        }

        // Trong IEventRepository/EventRepository
        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _context.Events
                .Include(e => e.Organization)
                .Include(e => e.Category)
                .Include(e => e.Registrations) // 🔴 THÊM dòng này
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Event>> GetEventsByOrganizationIdAsync(int organizationId)
        {
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Organization)
                .Where(e => e.OrganizationId == organizationId)
                .ToListAsync();
        }

        public async Task<Volunteer> GetVolunteerByUserIdAsync(string userId)
        {
            return await _context.Volunteers.FirstOrDefaultAsync(v => v.UserId == userId);
        }

        public async Task AddVolunteerAsync(Volunteer volunteer)
        {
            _context.Volunteers.Add(volunteer);
            await _context.SaveChangesAsync();
        }

        public async Task<EventRegistration?> GetRegistrationAsync(int eventId, string volunteerId)
        {
            // Chuyển volunteerId (string) → int
            if (!int.TryParse(volunteerId, out int volId))
                return null;

            return await _context.EventRegistrations
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.VolunteerId == volId);
        }
        public async Task<int> GetRegistrationCountAsync(int eventId)
        {
            return await _context.EventRegistrations.CountAsync(r => r.EventId == eventId);
        }

        public async Task AddRegistrationAsync(EventRegistration registration)
        {
            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync();
        }

        public async Task AddEventAsync(Event evt)
        {
            _context.Events.Add(evt);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}