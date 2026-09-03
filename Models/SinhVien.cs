using System.ComponentModel.DataAnnotations;

namespace DemoMVC.Models;

public class SinhVien
{
    [Display(Name = "Mã sinh viên")]
    [Required(ErrorMessage = "Vui lòng nhập mã sinh viên")]
    [StringLength(10, ErrorMessage = "Mã sinh viên tối đa 10 ký tự")]
    public string MaSV { get; set; } = "";

    [Display(Name = "Họ tên")]
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 50 ký tự")]
    public string HoTen { get; set; } = "";

    [Display(Name = "Tuổi")]
    [Range(16, 60, ErrorMessage = "Tuổi phải từ 16 đến 60")]
    public int Tuoi { get; set; }

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string? Email { get; set; }

    [Display(Name = "Khoa")]
    public string Khoa { get; set; } = "CNTT";
}
