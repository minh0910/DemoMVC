using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoMVC.Models
{
    // Bảng Sinh viên
    public class Student
    {
        // Mã sinh viên sinh tự động khi thêm mới, ví dụ 2821050199:
        // 28 = năm nhập học 2028, 21 = hệ đào tạo, 05 = ngành, 0199 = số thứ tự
        [Key]
        [Display(Name = "Mã sinh viên")]
        [StringLength(10)]
        public string StudentCode { get; set; } = string.Empty;

        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 50 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Ngày sinh")]
        [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [Column(TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }   // để nullable cho form trống ban đầu, [Required] vẫn làm cột NOT NULL

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Display(Name = "Năm nhập học")]
        [Required(ErrorMessage = "Vui lòng nhập năm nhập học")]
        [Range(2000, 2099, ErrorMessage = "Năm nhập học từ 2000 đến 2099")]
        public int EnrollmentYear { get; set; }

        // Mã hệ đào tạo 2 chữ số, danh sách trong TrainingSystems
        [Display(Name = "Hệ đào tạo")]
        [Required(ErrorMessage = "Vui lòng chọn hệ đào tạo")]
        [StringLength(2)]
        public string TrainingSystemCode { get; set; } = string.Empty;

        // Khoá ngoại: Students.FacultyCode -> Faculties.FacultyCode
        [Display(Name = "Khoa")]
        [Required(ErrorMessage = "Vui lòng chọn khoa")]
        [StringLength(10)]
        public string FacultyCode { get; set; } = string.Empty;

        public Faculty? Faculty { get; set; }

        // Khoá ngoại: Students.MajorCode -> Majors.MajorCode
        [Display(Name = "Ngành")]
        [Required(ErrorMessage = "Vui lòng chọn ngành")]
        [StringLength(2)]
        public string MajorCode { get; set; } = string.Empty;

        public Major? Major { get; set; }
    }
}
