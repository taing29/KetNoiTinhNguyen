using System.ComponentModel.DataAnnotations;

namespace TinhNguyenXanh.Models.ViewModel
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        public string? Subject { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        [MinLength(10, ErrorMessage = "Nội dung phải ít nhất 10 ký tự")]
        public string Message { get; set; } = string.Empty;
    }
}
