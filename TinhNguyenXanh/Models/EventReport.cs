using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TinhNguyenXanh.Data;

namespace TinhNguyenXanh.Models
{
    public class EventReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public Event Event { get; set; }

        // Khóa ngoại: Cột lưu trữ ID của người báo cáo
        [Required]
        public string ReporterUserId { get; set; }

        // 🌟 THÊM MỚI (Navigation Property) 🌟
        // Thuộc tính này cho phép .Include(r => r.User) hoạt động
        [ForeignKey("ReporterUserId")] // Liên kết với ReporterUserId
        public ApplicationUser User { get; set; }

        [Required, StringLength(500)]
        public string ReportReason { get; set; }

        public DateTime ReportDate { get; set; } = DateTime.Now;

        [Required, StringLength(50)]
        public string Status { get; set; } = "Pending";
    }
}
