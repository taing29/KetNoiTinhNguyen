using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TinhNguyenXanh.Data;

namespace TinhNguyenXanh.Models
{
    public class Organization
    {
        [Key]
        public int Id { get; set; }

        // ========== USER RELATIONSHIP ==========
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        // ========== THÔNG TIN CƠ BẢN ==========
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string OrganizationType { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }
        // ========== LĨNH VỰC HOẠT ĐỘNG ==========
        [Required]
        [MaxLength(500)]
        public string FocusAreas { get; set; } = string.Empty; // Store as comma-separated

        // ========== THÔNG TIN LIÊN HỆ ==========
        [Required]
        [MaxLength(200)]
        [EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Website { get; set; }

        // ========== ĐỊA CHỈ ==========
        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? City { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? District { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Ward { get; set; }

        // ========== THÔNG TIN PHÁP LÝ ==========
        [MaxLength(20)]
        public string? TaxCode { get; set; }

        public DateTime? FoundedDate { get; set; }

        [MaxLength(200)]
        public string? LegalRepresentative { get; set; }

        // ========== XÁC MINH ==========
        [MaxLength(500)]
        public string? VerificationDocsUrl { get; set; }

        [MaxLength(100)]
        public string? DocumentType { get; set; }

        public bool Verified { get; set; } = false;

        public DateTime? VerifiedDate { get; set; }

        [MaxLength(500)]
        public string? VerificationNotes { get; set; } // Admin notes

        // ========== MẠNG XÃ HỘI ==========
        [MaxLength(300)]
        public string? FacebookUrl { get; set; }

        [MaxLength(300)]
        public string? InstagramUrl { get; set; }

        [MaxLength(20)]
        public string? ZaloNumber { get; set; }

        // ========== THỐNG KÊ & KINH NGHIỆM ==========
        public int? MemberCount { get; set; }

        public int? EventsOrganized { get; set; }

        [MaxLength(1000)]
        public string? Achievements { get; set; }

        // ========== HỆ THỐNG ==========
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdated { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsApproved { get; set; } = false;

        // ========== RATING & REVIEWS ==========
        [Column(TypeName = "decimal(3,2)")]
        public decimal? AverageRating { get; set; }

        public int? TotalReviews { get; set; }

        // ========== RELATIONSHIPS ==========
        public virtual ICollection<Event>? Events { get; set; }
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}