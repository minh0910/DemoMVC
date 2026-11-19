namespace DemoMVC.Services
{
    // =========================
    // Bài 13: cấu hình gửi mail, đọc từ mục "Smtp" trong appsettings
    // Không commit mật khẩu: máy cá nhân dùng User Secrets, hosting lấy từ GitHub secret
    // =========================
    public class SmtpOptions
    {
        public string? Host { get; set; }
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string SenderEmail { get; set; } = "no-reply@demomvc.vn";
        public string SenderName { get; set; } = "DemoMVC";

        // Chưa khai báo Host thì không gửi thật mà lưu email ra file .eml trong thư mục này để kiểm tra
        public string PickupFolder { get; set; } = "App_Data/Mails";
    }
}
