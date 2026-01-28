using TinhNguyenXanh.DTOs;

namespace TinhNguyenXanh.Interfaces
{
    public interface IEventService
    {
        Task<IEnumerable<EventDTO>> GetAllEventsAsync();
        Task<EventDTO?> GetEventByIdAsync(int id);
        Task<IEnumerable<EventDTO>> GetApprovedEventsAsync();
        Task<bool> RegisterForEventAsync(int eventId, string userId);
        Task<IEnumerable<EventDTO>> GetEventsByOrganizationAsync(int organizationId);
        Task<bool> CreateEventAsync(EventDTO eventDto, int organizationId);

        Task<bool> UpdateEventAsync(EventDTO eventDto, int organizationId);

    }

}
