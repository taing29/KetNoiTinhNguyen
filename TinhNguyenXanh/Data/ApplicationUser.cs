using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Data
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }
        public string? AvatarPath { get; set; }
        public DateTime RegisteredDate { get; set; } = DateTime.UtcNow;
        public ICollection<EventFavorite> FavoriteEvents { get; set; } = new List<EventFavorite>();
        public ICollection<EventReport> SubmittedReports { get; set; } = new List<EventReport>();
    }
}
