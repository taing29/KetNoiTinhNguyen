using System.ComponentModel.DataAnnotations;

namespace TinhNguyenXanh.DTOs
{
    public class OrganizationDTO
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public string? UserName { get; set; }

        // ========== THÔNG TIN CƠ BẢN ==========

        [Required(ErrorMessage = "Tên tổ chức là bắt buộc")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tên tổ chức phải từ 3 đến 200 ký tự")]
        [Display(Name = "Tên tổ chức")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại tổ chức là bắt buộc")]
        [Display(Name = "Loại tổ chức")]
        public string OrganizationType { get; set; } = string.Empty;
        // Options: "NGO/Phi lợi nhuận", "Nhóm tình nguyện", "Doanh nghiệp xã hội", "Trường học/Đại học", "Cơ quan nhà nước", "Khác"

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [StringLength(2000, MinimumLength = 50, ErrorMessage = "Mô tả phải từ 50 đến 2000 ký tự")]
        [Display(Name = "Mô tả tổ chức")]
        public string Description { get; set; } = string.Empty;
        [Display(Name = "Logo/Ảnh đại diện tổ chức")]
        public IFormFile? AvatarFile { get; set; }

        public string? AvatarUrl { get; set; }

        // ========== LĨNH VỰC HOẠT ĐỘNG ==========

        [Display(Name = "Lĩnh vực hoạt động")]
        public List<string> FocusAreas { get; set; } = new List<string>();
        // Options: "Môi trường", "Giáo dục", "Y tế", "Trẻ em", "Người cao tuổi", "Người khuyết tật", "Cộng đồng", "Động vật", "Văn hóa"

        // ========== THÔNG TIN LIÊN HỆ ==========

        [Required(ErrorMessage = "Email liên hệ là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email liên hệ chính thức")]
        public string ContactEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^(\+84|0)[0-9]{9,10}$", ErrorMessage = "Số điện thoại phải là số Việt Nam hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Url(ErrorMessage = "URL không hợp lệ")]
        [Display(Name = "Website (nếu có)")]
        public string? Website { get; set; }

        // ========== ĐỊA CHỈ ==========

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được quá 500 ký tự")]
        [Display(Name = "Địa chỉ trụ sở")]
        public string Address { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Vui lòng chọn tỉnh/thành phố")]
        [Display(Name = "Tỉnh/Thành phố")]
        public string? City { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Vui lòng chọn quận/huyện")]
        [Display(Name = "Quận/Huyện")]
        public string? District { get; set; } = string.Empty;

        [Display(Name = "Phường/Xã")]
        public string? Ward { get; set; }

        // ========== THÔNG TIN PHÁP LÝ ==========

        [Display(Name = "Mã số thuế/ĐKKD")]
        [StringLength(20, ErrorMessage = "Mã số không được quá 20 ký tự")]
        public string? TaxCode { get; set; }

        [Display(Name = "Ngày thành lập")]
        [DataType(DataType.Date)]
        public DateTime? FoundedDate { get; set; }

        [Display(Name = "Người đại diện pháp luật")]
        [StringLength(200, ErrorMessage = "Tên không được quá 200 ký tự")]
        public string? LegalRepresentative { get; set; }

        // ========== XÁC MINH ==========

        [Url(ErrorMessage = "URL không hợp lệ")]
        [Display(Name = "Link tài liệu xác minh")]
        public string? VerificationDocsUrl { get; set; }

        [Display(Name = "Loại tài liệu")]
        public string? DocumentType { get; set; }
        // Options: "Giấy phép hoạt động", "Giấy đăng ký kinh doanh", "Quyết định thành lập", "Khác"

        // ========== MẠNG XÃ HỘI ==========

        [Display(Name = "Facebook Page")]
        public string? FacebookUrl { get; set; }

        [Display(Name = "Instagram")]
        public string? InstagramUrl { get; set; }

        [Display(Name = "Zalo")]
        [Phone(ErrorMessage = "Số Zalo không hợp lệ")]
        public string? ZaloNumber { get; set; }

        // ========== THỐNG KÊ & KINH NGHIỆM ==========

        [Range(0, 100000, ErrorMessage = "Số lượng không hợp lệ")]
        [Display(Name = "Số thành viên hiện tại")]
        public int? MemberCount { get; set; }

        [Range(0, 10000, ErrorMessage = "Số lượng không hợp lệ")]
        [Display(Name = "Số sự kiện đã tổ chức")]
        public int? EventsOrganized { get; set; }

        [StringLength(1000, ErrorMessage = "Không được quá 1000 ký tự")]
        [Display(Name = "Thành tích nổi bật")]
        public string? Achievements { get; set; }

        // ========== HỆ THỐNG ==========

        public DateTime JoinedDate { get; set; }
        public bool Verified { get; set; }
        public int? TotalReviews { get; set; }
        public bool IsApproved { get; set; } = false;
        public decimal? AverageRating { get; set; }
        //[Display(Name = "Tôi cam kết tuân thủ các quy định của nền tảng")]
        //[Range(typeof(bool), "true", "true", ErrorMessage = "Bạn phải đồng ý với các điều khoản")]
        //public bool AgreedToTerms { get; set; }
        public List<EventDTO> Events { get; set; }
        public List<ReviewDTO> Reviews { get; set; } = new List<ReviewDTO>();
    }
}