// Models/Review.cs
using System.ComponentModel.DataAnnotations;
using TinhNguyenXanh.Data;

namespace TinhNguyenXanh.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int OrganizationId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
        public virtual Organization? Organization { get; set; }
    }
}