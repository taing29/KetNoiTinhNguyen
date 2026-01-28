using System.ComponentModel.DataAnnotations;
namespace TinhNguyenXanh.Models
{
    public class Event
    {
        public int Id { get; set; }
        [Required, StringLength(150)]
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; }
        public string? LocationCoords { get; set; }
        //public string RequiredSkills { get; set; } // or relation
        public int MaxVolunteers { get; set; }
        public int? OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public int? CategoryId { get; set; }
        public EventCategory Category { get; set; }
        public string Status { get; set; } = "draft"; // draft/pending/approved/completed
        public string? Images { get; set; }
        public bool IsHidden { get; set; } = false;
        public string? HiddenReason { get; set; }
        public DateTime? HiddenAt { get; set; }
        //public string FinancialReport { get; set; }
        public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
        //public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public ICollection<EventFavorite> Favorites { get; set; } = new List<EventFavorite>();
        public ICollection<EventReport> Reports { get; set; } = new List<EventReport>();
    }
}