using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _repo;
        private readonly ApplicationDbContext _context;
        private readonly IEventRegistrationService _registrationService;

        public EventService(
            IEventRepository repo,
            ApplicationDbContext context,
            IEventRegistrationService registrationService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));
        }

        public async Task<IEnumerable<EventDTO>> GetAllEventsAsync()
        {
            var events = await _repo.GetAllEventsAsync();
            return events.Select(e => new EventDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Status = e.Status,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                OrganizationName = e.Organization?.Name ?? "Unknown",
                CategoryName = e.Category?.Name ?? "Uncategorized",
                RegisteredCount = e.Registrations?.Count ?? 0,
                MaxVolunteers = e.MaxVolunteers,
                Images = e.Images
            });
        }

        public async Task<IEnumerable<EventDTO>> GetEventsByOrganizationAsync(int organizationId)
        {
            var events = await _repo.GetEventsByOrganizationIdAsync(organizationId);
            return events.Select(e => new EventDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Status = e.Status,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Location = e.Location,
                CategoryName = e.Category?.Name ?? "Uncategorized",
                RegisteredCount = e.Registrations?.Count ?? 0,
                MaxVolunteers = e.MaxVolunteers,
            });
        }

        public async Task<EventDTO?> GetEventByIdAsync(int id)
        {
            var e = await _repo.GetEventByIdAsync(id);
            if (e == null) return null;

            // 🔴 THÊM: Đếm số registrations từ database
            var registeredCount = await _context.EventRegistrations
                .CountAsync(r => r.EventId == id && r.Status == "Confirmed");

            return new EventDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Status = e.Status,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Location = e.Location,
                LocationCoords = e.LocationCoords, // 🔴 THÊM
                OrganizationId = e.OrganizationId,
                OrganizationName = e.Organization?.Name ?? "Unknown",
                CategoryName = e.Category?.Name ?? "Uncategorized",
                RegisteredCount = registeredCount, // 🔴 SỬA
                MaxVolunteers = e.MaxVolunteers,
                Images = e.Images, // 🔴 THÊM
                CategoryId = e.CategoryId // 🔴 THÊM
            };
        }

        public async Task<IEnumerable<EventDTO>> GetApprovedEventsAsync()
        {
            var events = await _repo.GetAllEventsAsync();
            var approved = events.Where(e =>
                string.Equals(e.Status, "approved", StringComparison.OrdinalIgnoreCase));

            return approved.Select(e => new EventDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Status = e.Status,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Location = e.Location,
                OrganizationName = e.Organization?.Name ?? "Unknown",
                CategoryName = e.Category?.Name ?? "Uncategorized",
                RegisteredCount = e.Registrations?.Count ?? 0,
                MaxVolunteers = e.MaxVolunteers,
                Images = e.Images
            });
        }

        



        public async Task<bool> RegisterForEventAsync(int eventId, string userId)
        {
            var volunteer = await _context.Volunteers
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (volunteer == null) return false;

            var evt = await _context.Events.FindAsync(eventId);
            if (evt == null || !string.Equals(evt.Status, "approved", StringComparison.OrdinalIgnoreCase))
                return false;

            var dto = new EventRegistrationDTO
            {
                EventId = eventId,
                FullName = volunteer.FullName,
                Phone = volunteer.Phone ?? "",
                Reason = "Tham gia tình nguyện"
            };

            return await _registrationService.RegisterAsync(dto, userId);
        }

        public async Task<bool> CreateEventAsync(EventDTO eventDto, int organizationId)
        {
            if (eventDto == null) return false;

            string? imagePath = null;
            if (eventDto.ImageFile != null && eventDto.ImageFile.Length > 0)
            {
                try
                {
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(eventDto.ImageFile.FileName)}";
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/events");
                    Directory.CreateDirectory(folderPath);
                    var filePath = Path.Combine(folderPath, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await eventDto.ImageFile.CopyToAsync(stream);

                    imagePath = $"/images/events/{fileName}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[IMAGE ERROR] {ex.Message}");
                    return false;
                }
            }

            var newEvent = new Event
            {
                Title = eventDto.Title,
                Description = eventDto.Description,
                Location = eventDto.Location,
                LocationCoords = eventDto.LocationCoords,
                StartTime = eventDto.StartTime,
                EndTime = eventDto.EndTime,
                MaxVolunteers = eventDto.MaxVolunteers,
                OrganizationId = organizationId,
                CategoryId = eventDto.CategoryId,
                Status = "pending",
                Images = imagePath
            };

            try
            {
                await _repo.AddEventAsync(newEvent);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB ERROR] {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[INNER] {ex.InnerException.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateEventAsync(EventDTO eventDto, int organizationId)
        {
            if (eventDto == null || eventDto.Id <= 0) return false;

            var existingEvent = await _repo.GetEventByIdAsync(eventDto.Id);
            if (existingEvent == null || existingEvent.OrganizationId != organizationId)
                return false;

            string? imagePath = existingEvent.Images;
            if (eventDto.ImageFile != null && eventDto.ImageFile.Length > 0)
            {
                try
                {
                    if (!string.IsNullOrEmpty(existingEvent.Images))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingEvent.Images.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }

                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(eventDto.ImageFile.FileName)}";
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/events");
                    Directory.CreateDirectory(folderPath);
                    var filePath = Path.Combine(folderPath, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await eventDto.ImageFile.CopyToAsync(stream);

                    imagePath = $"/images/events/{fileName}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[IMAGE UPDATE ERROR] {ex.Message}");
                    return false;
                }
            }

            existingEvent.Title = eventDto.Title;
            existingEvent.Description = eventDto.Description;
            existingEvent.Location = eventDto.Location;
            existingEvent.LocationCoords = eventDto.LocationCoords;
            existingEvent.StartTime = eventDto.StartTime;
            existingEvent.EndTime = eventDto.EndTime;
            existingEvent.MaxVolunteers = eventDto.MaxVolunteers;
            existingEvent.CategoryId = eventDto.CategoryId;
            existingEvent.Images = imagePath;

            try
            {
                await _repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UPDATE DB ERROR] {ex.Message}");
                return false;
            }
        }
    }
}