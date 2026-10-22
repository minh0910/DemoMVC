using System.ComponentModel.DataAnnotations;

namespace DemoMVC.Models
{
    // Bảng Khoa
    public class Faculty
    {
        // Khoá chính là mã khoa (CNTT, MO, ...) chứ không phải số tự tăng
        [Key]
        [Display(Name = "Mã khoa")]
        [Required(ErrorMessage = "Mã khoa không được để trống")]
        [StringLength(10, ErrorMessage = "Mã khoa tối đa 10 ký tự")]
        [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Mã khoa chỉ gồm chữ cái không dấu và chữ số")]
        public string FacultyCode { get; set; } = string.Empty;

        [Display(Name = "Tên khoa")]
        [Required(ErrorMessage = "Tên khoa không được để trống")]
        [StringLength(100, ErrorMessage = "Tên khoa tối đa 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        // Một khoa có nhiều ngành, nhiều sinh viên
        public ICollection<Major> Majors { get; set; } = new List<Major>();

        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
