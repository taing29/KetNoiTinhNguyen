// TinhNguyenXanh/Services/EmailService.cs
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string message,
            string? replyToEmail = null,
            string? replyToName = null)
        {
            var email = new MimeMessage();

            // Từ: Tên website + email của bạn
            email.From.Add(new MailboxAddress("Tình Nguyện Xanh", _emailSettings.SenderEmail));

            // SIÊU QUAN TRỌNG: Khi bấm Reply sẽ gửi thẳng cho người dùng
            if (!string.IsNullOrWhiteSpace(replyToEmail))
            {
                email.ReplyTo.Add(new MailboxAddress(replyToName ?? "Người dùng", replyToEmail));
            }

            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = message;
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LỖI GỬI EMAIL: {ex.Message}");
                throw;
            }
        }
    }
}