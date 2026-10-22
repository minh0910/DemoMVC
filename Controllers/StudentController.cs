using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DemoMVC.Data;
using DemoMVC.Models;
using DemoMVC.Services;

namespace DemoMVC.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StudentCodeGenerator _codeGenerator;

        public StudentController(ApplicationDbContext context, StudentCodeGenerator codeGenerator)
        {
            _context = context;
            _codeGenerator = codeGenerator;
        }

        // =========================
        // GET: Student?facultyCode=CNTT&keyword=an
        // =========================
        public async Task<IActionResult> Index(string? facultyCode, string? keyword)
        {
            var query = _context.Students
                .Include(s => s.Faculty)         // JOIN sang bảng Khoa qua FacultyCode
                .Include(s => s.Major)           // JOIN sang bảng Ngành qua MajorCode
                .AsQueryable();

            if (!string.IsNullOrEmpty(facultyCode))
            {
                query = query.Where(s => s.FacultyCode == facultyCode);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var tuKhoa = keyword.Trim();
                query = query.Where(s => s.FullName.Contains(tuKhoa) || s.StudentCode.Contains(tuKhoa));
            }

            ViewBag.FacultyList = new SelectList(
                await _context.Faculties.OrderBy(f => f.Name).ToListAsync(),
                nameof(Faculty.FacultyCode), nameof(Faculty.Name), facultyCode);
            ViewBag.Keyword = keyword;

            return View(await query.OrderBy(s => s.StudentCode).ToListAsync());
        }


        // =========================
        // GET: Student/Details/2821050199
        // =========================
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Faculty)
                .Include(s => s.Major)
                .FirstOrDefaultAsync(s => s.StudentCode == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }


        // =========================
        // GET: Student/Create
        // =========================
        public async Task<IActionResult> Create()
        {
            var student = new Student
            {
                EnrollmentYear = DateTime.Now.Year,
                TrainingSystemCode = "21",
            };

            await LoadDropDownsAsync(student);

            return View(student);
        }


        // =========================
        // POST: Student/Create
        // Không nhận StudentCode từ form: mã được sinh tự động ở server
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FullName,DateOfBirth,Email,EnrollmentYear,TrainingSystemCode,FacultyCode,MajorCode")] Student student)
        {
            ModelState.Remove(nameof(Student.StudentCode));

            await ValidateCodesAsync(student);

            if (ModelState.IsValid)
            {
                // Thử lại nếu 2 người thêm cùng lúc và lấy trùng mã (lỗi trùng khoá chính)
                for (var lanThu = 1; lanThu <= 3; lanThu++)
                {
                    var code = await _codeGenerator.GenerateAsync(student.EnrollmentYear, student.TrainingSystemCode, student.MajorCode);

                    if (code == null)
                    {
                        ModelState.AddModelError(string.Empty, "Đã dùng hết số thứ tự (9999) cho năm, hệ và ngành này");
                        break;
                    }

                    student.StudentCode = code;
                    _context.Students.Add(student);

                    try
                    {
                        await _context.SaveChangesAsync();

                        TempData["ThongBao"] = $"Đã thêm sinh viên {student.FullName}, mã sinh viên {student.StudentCode}";

                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateException) when (lanThu < 3)
                    {
                        _context.Entry(student).State = EntityState.Detached;
                    }
                }
            }

            await LoadDropDownsAsync(student);

            return View(student);
        }


        // =========================
        // GET: Student/NextCode?enrollmentYear=2028&trainingSystemCode=21&majorCode=05
        // Trả về mã dự kiến (JSON) để form thêm mới hiển thị trước
        // =========================
        public async Task<IActionResult> NextCode(int enrollmentYear, string? trainingSystemCode, string? majorCode)
        {
            if (enrollmentYear < 2000 || enrollmentYear > 2099
                || !TrainingSystems.IsValid(trainingSystemCode)
                || majorCode == null
                || !await _context.Majors.AnyAsync(m => m.MajorCode == majorCode))
            {
                return Json(new { code = (string?)null });
            }

            var code = await _codeGenerator.GenerateAsync(enrollmentYear, trainingSystemCode!, majorCode);

            return Json(new { code });
        }


        // =========================
        // GET: Student/Edit/2821050199
        // =========================
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Faculty)
                .Include(s => s.Major)
                .FirstOrDefaultAsync(s => s.StudentCode == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }


        // =========================
        // POST: Student/Edit/2821050199
        // Năm nhập học, hệ, khoa, ngành đã nằm trong mã sinh viên nên chỉ cho sửa thông tin cá nhân
        // =========================
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Faculty)
                .Include(s => s.Major)
                .FirstOrDefaultAsync(s => s.StudentCode == id);

            if (student == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync(student, "", s => s.FullName, s => s.DateOfBirth, s => s.Email))
            {
                await _context.SaveChangesAsync();

                TempData["ThongBao"] = $"Đã cập nhật sinh viên {student.StudentCode}";

                return RedirectToAction(nameof(Index));
            }

            return View(student);
        }


        // =========================
        // GET: Student/Delete/2821050199
        // =========================
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Faculty)
                .Include(s => s.Major)
                .FirstOrDefaultAsync(s => s.StudentCode == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }


        // =========================
        // POST: Student/Delete/2821050199
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student != null)
            {
                _context.Students.Remove(student);

                await _context.SaveChangesAsync();

                TempData["ThongBao"] = $"Đã xoá sinh viên {student.StudentCode} - {student.FullName}";
            }

            return RedirectToAction(nameof(Index));
        }


        // Kiểm tra hệ đào tạo hợp lệ và ngành đã chọn thuộc đúng khoa đã chọn
        private async Task ValidateCodesAsync(Student student)
        {
            if (!string.IsNullOrEmpty(student.TrainingSystemCode) && !TrainingSystems.IsValid(student.TrainingSystemCode))
            {
                ModelState.AddModelError(nameof(Student.TrainingSystemCode), "Hệ đào tạo không hợp lệ");
            }

            if (!string.IsNullOrEmpty(student.MajorCode))
            {
                var major = await _context.Majors.FindAsync(student.MajorCode);

                if (major == null)
                {
                    ModelState.AddModelError(nameof(Student.MajorCode), "Ngành không tồn tại");
                }
                else if (major.FacultyCode != student.FacultyCode)
                {
                    ModelState.AddModelError(nameof(Student.MajorCode), "Ngành đã chọn không thuộc khoa đã chọn");
                }
            }
        }

        // Dữ liệu cho các dropdown: hệ đào tạo, khoa, ngành (ngành lọc theo khoa bằng JS ở View)
        private async Task LoadDropDownsAsync(Student student)
        {
            ViewBag.TrainingSystemList = new SelectList(
                TrainingSystems.All.Select(t => new { Code = t.Key, Name = $"{t.Key} - {t.Value}" }),
                "Code", "Name", student.TrainingSystemCode);

            ViewBag.FacultyList = new SelectList(
                await _context.Faculties.Where(f => f.Majors.Any()).OrderBy(f => f.Name).ToListAsync(),
                nameof(Faculty.FacultyCode), nameof(Faculty.Name), student.FacultyCode);

            ViewBag.Majors = await _context.Majors
                .OrderBy(m => m.MajorCode)
                .Select(m => new { code = m.MajorCode, name = m.Name, facultyCode = m.FacultyCode })
                .ToListAsync();
        }
    }
}
