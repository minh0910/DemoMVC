using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DemoMVC.Models
{
    // =========================
    // Bài 13: tài khoản đăng nhập
    // Kế thừa IdentityUser (UserName, Email, PasswordHash...) và thêm cột Họ tên
    // =========================
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Họ tên")]
        [StringLength(100)]
        public string? FullName { get; set; }
    }
}
