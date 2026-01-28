
using System.ComponentModel.DataAnnotations;

namespace TinhNguyenXanh.Models
{
    public class EventCategory
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        public string Name { get; set; }
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
