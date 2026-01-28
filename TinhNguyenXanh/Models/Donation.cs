using System.ComponentModel.DataAnnotations;

namespace TinhNguyenXanh.Models
{
    public class Donation
    {
        [Key]
        public int Id { get; set; } // Mã này sẽ dùng làm nội dung chuyển khoản (VD: TNX 1005)

        public string DonorName { get; set; }
        public decimal Amount { get; set; }
        public string PhoneNumber { get; set; }

        public string? Message { get; set; }
        public bool IsPaid { get; set; } = false; // Mặc định là chưa thanh toán
        public string? TransactionCode { get; set; } // Mã giao dịch ngân hàng (lưu lại để đối soát)
        public DateTime CreatedAt { get; set; } = DateTime.Now;


    }
}