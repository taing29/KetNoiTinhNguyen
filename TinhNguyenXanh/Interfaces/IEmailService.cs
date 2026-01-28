// TinhNguyenXanh/Interfaces/IEmailService.cs
namespace TinhNguyenXanh.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(
            string toEmail,
            string subject,
            string message,
            string? replyToEmail = null,
            string? replyToName = null);
    }
}