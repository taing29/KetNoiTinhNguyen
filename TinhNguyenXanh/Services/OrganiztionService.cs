using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public OrganizationService(
            IOrganizationRepository repo,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _repo = repo;
            _userManager = userManager;
            _env = env;
        }

        public async Task<IEnumerable<OrganizationDTO>> GetAllAsync()
        {
            var orgs = await _repo.GetAllAsync();
            return orgs
                .Where(o => o.Verified)
                .Select(o => MapToDTO(o));
        }

        public async Task<OrganizationDTO?> GetByIdAsync(int id)
        {
            var o = await _repo.GetByIdAsync(id);
            if (o == null || !o.Verified) return null;

            return MapToDTO(o);
        }

        public async Task<bool> RegisterAsync(OrganizationDTO model, string userId)
        {
            try
            {
                // Validate input (bỏ AgreedToTerms)
                ValidateModel(model, userId);

                // Kiểm tra user tồn tại
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new InvalidOperationException($"Không tìm thấy user với ID: {userId}");
                }

                // Kiểm tra user đã có tổ chức chưa
                var existingOrg = await _repo.GetByUserIdAsync(userId);
                if (existingOrg != null)
                {
                    throw new InvalidOperationException("Bạn đã đăng ký tổ chức trước đó");
                }

                // Kiểm tra user đã là Organizer chưa
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains(SD.Role_Organizer))
                {
                    throw new InvalidOperationException("Bạn đã là Ban tổ chức");
                }

                // Upload avatar nếu có
                string? avatarUrl = null;
                if (model.AvatarFile != null)
                {
                    avatarUrl = await UploadAvatarAsync(model.AvatarFile);
                }

                // Tạo organization mới
                var organization = new Organization
                {
                    UserId = userId,

                    // Thông tin cơ bản
                    Name = model.Name.Trim(),
                    OrganizationType = model.OrganizationType.Trim(),
                    Description = model.Description.Trim(),
                    FocusAreas = string.Join(",", model.FocusAreas),
                    AvatarUrl = avatarUrl, // Thêm avatar

                    // Thông tin liên hệ
                    ContactEmail = model.ContactEmail.Trim(),
                    PhoneNumber = model.PhoneNumber.Trim(),
                    Website = model.Website?.Trim(),

                    // Địa chỉ
                    Address = model.Address.Trim(),
                    City = model.City.Trim(),
                    District = model.District.Trim(),
                    Ward = model.Ward?.Trim(),

                    // Thông tin pháp lý
                    TaxCode = model.TaxCode?.Trim(),
                    FoundedDate = model.FoundedDate,
                    LegalRepresentative = model.LegalRepresentative?.Trim(),

                    // Xác minh
                    VerificationDocsUrl = model.VerificationDocsUrl?.Trim(),
                    DocumentType = model.DocumentType?.Trim(),
                    Verified = true,

                    // Mạng xã hội
                    FacebookUrl = model.FacebookUrl?.Trim(),
                    ZaloNumber = model.ZaloNumber?.Trim(),

                    // Thống kê
                    MemberCount = model.MemberCount,
                    EventsOrganized = model.EventsOrganized,
                    Achievements = model.Achievements?.Trim(),

                    JoinedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow,
                    IsActive = true
                };

                Console.WriteLine($"[RegisterAsync] Creating organization for userId: {userId}");
                Console.WriteLine($"[RegisterAsync] Organization: Name={organization.Name}, Avatar={avatarUrl}");

                // Lưu vào database
                await _repo.AddAsync(organization);
                await _repo.SaveChangesAsync();

                //tạm thời bỏ gán role Organizer
                // Gán role Organizer
                //Console.WriteLine($"[RegisterAsync] Adding role {SD.Role_Organizer} to user {userId}");
                //var result = await _userManager.AddToRoleAsync(user, SD.Role_Organizer);

                //if (!result.Succeeded)
                //{
                //    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                //    Console.WriteLine($"[RegisterAsync] Failed to add role: {errors}");
                //    throw new InvalidOperationException($"Không thể gán role Organizer: {errors}");
                //}

                //Console.WriteLine("[RegisterAsync] Registration completed successfully");
                return true;
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"[RegisterAsync DbUpdateException] {dbEx.Message}");
                Console.WriteLine($"Inner Exception: {dbEx.InnerException?.Message}");

                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                if (innerMessage.Contains("FOREIGN KEY"))
                {
                    throw new InvalidOperationException("Lỗi liên kết dữ liệu. UserId không tồn tại trong hệ thống.");
                }
                else if (innerMessage.Contains("UNIQUE") || innerMessage.Contains("duplicate"))
                {
                    throw new InvalidOperationException("Tổ chức này đã tồn tại trong hệ thống.");
                }
                else if (innerMessage.Contains("NULL"))
                {
                    throw new InvalidOperationException("Thiếu thông tin bắt buộc. Vui lòng kiểm tra lại.");
                }
                else
                {
                    throw new InvalidOperationException($"Lỗi khi lưu dữ liệu: {innerMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RegisterAsync Error] {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        // Upload avatar
        private async Task<string> UploadAvatarAsync(IFormFile file)
        {
            try
            {
                // Validate file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException("Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif)");
                }

                if (file.Length > 5 * 1024 * 1024) // 5MB
                {
                    throw new InvalidOperationException("Kích thước file không được vượt quá 5MB");
                }

                // Create uploads folder
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "organizations");
                Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/images/organizations/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UploadAvatarAsync Error] {ex.Message}");
                throw new InvalidOperationException($"Lỗi khi upload avatar: {ex.Message}");
            }
        }

        private void ValidateModel(OrganizationDTO model, string userId)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                throw new ArgumentException("Tên tổ chức không được để trống");

            if (string.IsNullOrWhiteSpace(model.OrganizationType))
                throw new ArgumentException("Loại tổ chức không được để trống");

            if (string.IsNullOrWhiteSpace(model.Description) || model.Description.Length < 50)
                throw new ArgumentException("Mô tả phải có ít nhất 50 ký tự");

            if (model.FocusAreas == null || !model.FocusAreas.Any())
                throw new ArgumentException("Vui lòng chọn ít nhất một lĩnh vực hoạt động");

            if (string.IsNullOrWhiteSpace(model.ContactEmail))
                throw new ArgumentException("Email liên hệ không được để trống");

            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                throw new ArgumentException("Số điện thoại không được để trống");

            if (string.IsNullOrWhiteSpace(model.Address))
                throw new ArgumentException("Địa chỉ không được để trống");

            if (string.IsNullOrWhiteSpace(model.City))
                throw new ArgumentException("Tỉnh/Thành phố không được để trống");

            if (string.IsNullOrWhiteSpace(model.District))
                throw new ArgumentException("Quận/Huyện không được để trống");

            // Bỏ validation AgreedToTerms

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId không hợp lệ");
        }

        private OrganizationDTO MapToDTO(Organization o)
        {
            return new OrganizationDTO
            {
                Id = o.Id,
                UserId = o.UserId,
                UserName = o.User?.UserName,

                // Thông tin cơ bản
                Name = o.Name,
                OrganizationType = o.OrganizationType,
                Description = o.Description,
                FocusAreas = string.IsNullOrEmpty(o.FocusAreas)
                    ? new List<string>()
                    : o.FocusAreas.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                AvatarUrl = o.AvatarUrl, // Thêm avatar

                // Thông tin liên hệ
                ContactEmail = o.ContactEmail,
                PhoneNumber = o.PhoneNumber,
                Website = o.Website,

                // Địa chỉ
                Address = o.Address,
                City = o.City,
                District = o.District,
                Ward = o.Ward,

                // Thông tin pháp lý
                TaxCode = o.TaxCode,
                FoundedDate = o.FoundedDate,
                LegalRepresentative = o.LegalRepresentative,

                // Xác minh
                VerificationDocsUrl = o.VerificationDocsUrl,
                DocumentType = o.DocumentType,
                Verified = o.Verified,
                IsApproved = o.IsApproved,

                // Mạng xã hội
                FacebookUrl = o.FacebookUrl,
                ZaloNumber = o.ZaloNumber,
                InstagramUrl = o.InstagramUrl,

                // Thống kê
                MemberCount = o.MemberCount,
                EventsOrganized = o.EventsOrganized,
                Achievements = o.Achievements,

                JoinedDate = o.JoinedDate,
                TotalReviews = o.TotalReviews,
                AverageRating = o.AverageRating,

                // Thêm danh sách đánh giá
                Reviews = o.Reviews?
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDTO
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = !string.IsNullOrWhiteSpace(r.User?.FullName)
           ? r.User.FullName
           : (r.User?.UserName ?? "Tình nguyện viên"),
                AvatarUrl = r.User?.AvatarPath ?? "/images/default-avatar.png",
                Rating = r.Rating,
                Comment = r.Comment ?? "",
                CreatedAt = r.CreatedAt
            })
            .ToList() ?? new List<ReviewDTO>(),
                // 🆕 Map luôn các sự kiện của tổ chức
                Events = o.Events?
                .Where(e => e.Status == "approved")  // chỉ lấy event approved
                .Select(e => new EventDTO
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Status = e.Status,  // giữ nguyên status từ DB
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Location = e.Location,
                    LocationCoords = e.LocationCoords,
                    OrganizationName = o.Name,
                    CategoryName = e.Category?.Name,
                    MaxVolunteers = e.MaxVolunteers,
                    CategoryId = e.CategoryId,
                    OrganizationId = e.OrganizationId,
                    Images = e.Images
                })
                .ToList()
            };
        }
        // === THÊM HÀM NÀY VÀO OrganizationService ===
        private string? NormalizeUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            url = url.Trim();
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }
            return url;
        }
        // Trong OrganizationService.cs

        public async Task<OrganizationDTO?> GetByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var org = await _repo.GetByUserIdAsync(userId);
            return org == null ? null : MapToDTO(org);
        }

        public async Task<bool> UpdateAsync(OrganizationDTO model, string userId, IFormFile? avatarFile, IFormFile? docFile)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            var org = await _repo.GetByUserIdAsync(userId);
            if (org == null)
                return false;

            try
            {
                // === CẬP NHẬT THÔNG TIN ===
                org.Name = model.Name?.Trim() ?? org.Name;
                org.OrganizationType = model.OrganizationType?.Trim() ?? org.OrganizationType;
                org.Description = model.Description?.Trim() ?? org.Description;
                org.FocusAreas = model.FocusAreas?.Any() == true
                    ? string.Join(",", model.FocusAreas)
                    : org.FocusAreas;

                org.ContactEmail = model.ContactEmail?.Trim() ?? org.ContactEmail;
                org.PhoneNumber = model.PhoneNumber?.Trim() ?? org.PhoneNumber;
                org.Website = model.Website?.Trim();
                org.Address = model.Address?.Trim() ?? org.Address;
                org.City = model.City?.Trim();
                org.District = model.District?.Trim();
                org.Ward = model.Ward?.Trim();
                org.TaxCode = model.TaxCode?.Trim();
                org.FoundedDate = model.FoundedDate;
                org.LegalRepresentative = model.LegalRepresentative?.Trim();
                org.DocumentType = model.DocumentType?.Trim();
                org.FacebookUrl = NormalizeUrl(model.FacebookUrl);
                org.InstagramUrl = NormalizeUrl(model.InstagramUrl);
                org.ZaloNumber = model.ZaloNumber?.Trim();
                org.MemberCount = model.MemberCount;
                org.EventsOrganized = model.EventsOrganized;
                org.Achievements = model.Achievements?.Trim();
                org.LastUpdated = DateTime.UtcNow;

                // === UPLOAD AVATAR (nếu có) ===
                if (avatarFile != null && avatarFile.Length > 0)
                {
                    // XÓA FILE CŨ (nếu có)
                    if (!string.IsNullOrEmpty(org.AvatarUrl))
                    {
                        DeleteFile(org.AvatarUrl);
                    }

                    org.AvatarUrl = await UploadFileAsync(avatarFile, "avatar");
                }

                // === UPLOAD TÀI LIỆU (nếu có) ===
                if (docFile != null && docFile.Length > 0)
                {
                    // XÓA FILE CŨ (nếu có)
                    if (!string.IsNullOrEmpty(org.VerificationDocsUrl))
                    {
                        DeleteFile(org.VerificationDocsUrl);
                    }

                    org.VerificationDocsUrl = await UploadFileAsync(docFile, "doc");
                }

                await _repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Ghi log nếu cần
                Console.WriteLine($"[UpdateAsync Error] {ex.Message}");
                return false;
            }
        }

        // === UPLOAD FILE RIÊNG BIỆT ===
        private async Task<string> UploadFileAsync(IFormFile file, string type)
        {
            // Kiểm tra loại file
            var allowedAvatar = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var allowedDoc = new[] { ".pdf", ".doc", ".docx" };

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{ext}";
            string filePath, urlPath;

            if (type == "avatar")
            {
                if (!allowedAvatar.Contains(ext))
                    throw new InvalidOperationException("Chỉ chấp nhận ảnh: jpg, png, gif");
                if (file.Length > 5 * 1024 * 1024)
                    throw new InvalidOperationException("Avatar không quá 5MB");

                filePath = Path.Combine(_env.WebRootPath, "images", "organizations", fileName);
                urlPath = $"/images/organizations/{fileName}";
            }
            else if (type == "doc")
            {
                if (!allowedDoc.Contains(ext))
                    throw new InvalidOperationException("Chỉ chấp nhận: PDF, Word");
                if (file.Length > 20 * 1024 * 1024)
                    throw new InvalidOperationException("Tài liệu không quá 20MB");

                filePath = Path.Combine(_env.WebRootPath, "uploads", "docs", fileName);
                urlPath = $"/uploads/docs/{fileName}";
            }
            else
            {
                throw new ArgumentException("Loại file không hợp lệ");
            }

            // Tạo thư mục nếu chưa có
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            // Lưu file
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return urlPath;
        }

        // === XÓA FILE CŨ ===
        private void DeleteFile(string? fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            try
            {
                var filePath = Path.Combine(_env.WebRootPath, fileUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteFile Error] {ex.Message}");
                // Không ném lỗi → không làm hỏng update
            }
        }

        // OrganizationService.cs – thêm vào cuối class
        public async Task<bool> HasUserReviewedAsync(int organizationId, string userId)
        {
            var org = await _repo.GetByIdAsync(organizationId);
            if (org == null) return false;

            return org.Reviews?.Any(r => r.UserId == userId) == true;
        }

        public async Task<bool> AddReviewAsync(int organizationId, string userId, int rating, string comment)
        {
            var org = await _repo.GetByIdAsync(organizationId);
            if (org == null || !org.Verified) return false;

            // Kiểm tra đã review chưa
            if (org.Reviews?.Any(r => r.UserId == userId) == true)
                return false;

            var review = new Review
            {
                OrganizationId = organizationId,
                UserId = userId,
                Rating = rating,
                Comment = comment?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            org.Reviews ??= new List<Review>();
            org.Reviews.Add(review);

            // Cập nhật thống kê đánh giá
            org.TotalReviews = (org.TotalReviews ?? 0) + 1;
            org.AverageRating = Math.Round(
                ((org.AverageRating ?? 0) * (org.TotalReviews.Value - 1) + rating) / org.TotalReviews.Value,
                1);

            await _repo.SaveChangesAsync();
            return true;
        }
    }
}