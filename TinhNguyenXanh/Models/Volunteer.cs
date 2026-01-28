using System.ComponentModel.DataAnnotations;
using TinhNguyenXanh.Data;

namespace TinhNguyenXanh.Models
{
    public class Volunteer
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? Skills { get; set; } // kỹ năng

        public string? Bio { get; set; }

        public string? AvatarUrl { get; set; }

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
        public string Availability { get; set; } = "Available"; // Available, Busy, Inactive
    }
}