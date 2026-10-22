using Microsoft.EntityFrameworkCore;
using DemoMVC.Data;

namespace DemoMVC.Services
{
    // Sinh mã sinh viên 10 chữ số: [2 số cuối năm nhập học][mã hệ đào tạo][mã ngành][số thứ tự 4 chữ số]
    // Ví dụ: năm 2028, hệ 21, ngành 05, số thứ tự 199 => 2821050199; sinh viên tiếp theo => 2821050200
    public class StudentCodeGenerator
    {
        public const int SequenceLength = 4;

        private readonly ApplicationDbContext _context;

        public StudentCodeGenerator(ApplicationDbContext context)
        {
            _context = context;
        }

        // 6 chữ số đầu của mã, ví dụ năm 2028, hệ 21, ngành 05 => "282105"
        public static string GetPrefix(int enrollmentYear, string trainingSystemCode, string majorCode)
        {
            return $"{enrollmentYear % 100:D2}{trainingSystemCode}{majorCode}";
        }

        // Trả về mã tiếp theo cho nhóm (năm, hệ, ngành); null nếu đã dùng hết số thứ tự 9999
        public async Task<string?> GenerateAsync(int enrollmentYear, string trainingSystemCode, string majorCode)
        {
            var prefix = GetPrefix(enrollmentYear, trainingSystemCode, majorCode);

            // Mã cùng nhóm có cùng độ dài nên sắp xếp theo chuỗi cũng là sắp xếp theo số
            var lastCode = await _context.Students
                .Where(s => s.StudentCode.StartsWith(prefix))
                .OrderByDescending(s => s.StudentCode)
                .Select(s => s.StudentCode)
                .FirstOrDefaultAsync();

            var next = 1;
            if (lastCode != null && int.TryParse(lastCode.Substring(prefix.Length), out var lastNumber))
            {
                next = lastNumber + 1;
            }

            if (next > 9999)
            {
                return null;
            }

            return prefix + next.ToString("D" + SequenceLength);
        }
    }
}
