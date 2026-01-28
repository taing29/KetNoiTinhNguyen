using System.Collections.Generic;
using System.Threading.Tasks;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Interfaces
{
    public interface IEventRepository
    {
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<Event> GetEventByIdAsync(int id);

        Task<Volunteer?> GetVolunteerByUserIdAsync(string userId);
        Task AddVolunteerAsync(Volunteer volunteer);

        Task<EventRegistration?> GetRegistrationAsync(int eventId, string volunteerId);
        Task<int> GetRegistrationCountAsync(int eventId);
        Task AddRegistrationAsync(EventRegistration registration);
        Task<IEnumerable<Event>> GetEventsByOrganizationIdAsync(int organizationId);
        Task AddEventAsync(Event evt);
        Task SaveChangesAsync();

    }
}
