using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace DemoMVC.Services
{
    // =========================
    // Bài 13: Identity gọi IEmailSender khi cần gửi link xác thực tài khoản / đặt lại mật khẩu
    // =========================
    public class EmailSender : IEmailSender
    {
        private readonly SmtpOptions _smtp;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<SmtpOptions> smtp, IWebHostEnvironment env, ILogger<EmailSender> logger)
        {
            _smtp = smtp.Value;
            _env = env;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using var mail = new MailMessage
            {
                From = new MailAddress(_smtp.SenderEmail, _smtp.SenderName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mail.To.Add(email);

            using var client = new SmtpClient();

            if (string.IsNullOrWhiteSpace(_smtp.Host))
            {
                // Chưa cấu hình SMTP → ghi email ra file
                var thuMuc = Path.Combine(_env.ContentRootPath, _smtp.PickupFolder);
                Directory.CreateDirectory(thuMuc);
                client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                client.PickupDirectoryLocation = thuMuc;
                _logger.LogWarning("Chưa cấu hình Smtp:Host, email \"{Subject}\" tới {Email} được lưu ở {Folder}", subject, email, thuMuc);
            }
            else
            {
                client.Host = _smtp.Host;
                client.Port = _smtp.Port;
                client.EnableSsl = _smtp.EnableSsl;
                client.Credentials = new NetworkCredential(_smtp.UserName, _smtp.Password);
            }

            await client.SendMailAsync(mail);
        }
    }
}
