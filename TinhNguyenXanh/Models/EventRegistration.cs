using System.ComponentModel.DataAnnotations;

namespace TinhNguyenXanh.Models
{
    public class EventRegistration
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        public int VolunteerId { get; set; }
        public Volunteer Volunteer { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Rejected

        public DateTime RegisteredDate { get; set; } = DateTime.UtcNow;
    }
}