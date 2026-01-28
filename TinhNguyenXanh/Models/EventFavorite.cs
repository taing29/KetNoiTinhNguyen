using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TinhNguyenXanh.Data;

namespace TinhNguyenXanh.Models
{
    public class EventFavorite
    {
        [Required]
        public int EventId { get; set; }
        public Event Event { get; set; }

        [Required]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public DateTime FavoriteDate { get; set; } = DateTime.Now;
    }
}
