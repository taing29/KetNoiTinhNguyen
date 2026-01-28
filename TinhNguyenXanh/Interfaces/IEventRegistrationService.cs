using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Interfaces
{
    public interface IEventRegistrationService
    {
        Task<bool> RegisterAsync(EventRegistrationDTO dto, string userId);
        Task<EventRegistration?> GetByIdAsync(int id);
        Task<IEnumerable<EventRegistration>> GetByEventIdAsync(int eventId);
        Task<IEnumerable<EventRegistration>> GetByVolunteerIdAsync(int volunteerId);
        Task<bool> ApproveAsync(int id);
        Task<bool> RejectAsync(int id);
    }
}