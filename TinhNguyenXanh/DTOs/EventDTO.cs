using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.DTOs
{
    public class EventDTO
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; }
        public string? LocationCoords { get; set; }
        public string? OrganizationName { get; set; }
        public string? CategoryName { get; set; }
        public int? RegisteredCount { get; set; }
        public int MaxVolunteers { get; set; }
        public int? CategoryId { get; set; }
        public int? OrganizationId { get; set; }
        public IFormFile? ImageFile { get; set; } // 🆕 cho upload ảnh
        public string? Images { get; set; }
        public bool IsHidden { get; set; } = false;
        public bool IsFavorited { get; set; }
        public IEnumerable<EventDTO>? RelatedEvents { get; set; }


    }
}
