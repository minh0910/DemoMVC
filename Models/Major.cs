using System.ComponentModel.DataAnnotations;

namespace DemoMVC.Models
{
    // Bảng Ngành học
    public class Major
    {
        // Mã ngành 2 chữ số, là phần thứ 3 của mã sinh viên (ví dụ 05 = Công nghệ thông tin)
        [Key]
        [Display(Name = "Mã ngành")]
        [Required(ErrorMessage = "Mã ngành không được để trống")]
        [StringLength(2)]
        [RegularExpression(@"^\d{2}$", ErrorMessage = "Mã ngành gồm đúng 2 chữ số")]
        public string MajorCode { get; set; } = string.Empty;

        [Display(Name = "Tên ngành")]
        [Required(ErrorMessage = "Tên ngành không được để trống")]
        [StringLength(100, ErrorMessage = "Tên ngành tối đa 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        // Khoá ngoại: Majors.FacultyCode -> Faculties.FacultyCode
        [Display(Name = "Khoa")]
        [Required(ErrorMessage = "Vui lòng chọn khoa")]
        [StringLength(10)]
        public string FacultyCode { get; set; } = string.Empty;

        public Faculty? Faculty { get; set; }

        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
