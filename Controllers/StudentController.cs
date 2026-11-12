using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DemoMVC.Data;
using DemoMVC.Models;
using DemoMVC.Services;

namespace DemoMVC.Controllers
{
    // Bài 11: CRUD sinh viên bằng Modal + Ajax
    public class StudentController : AjaxCrudController
    {
        private readonly ApplicationDbContext _context;
        private readonly StudentCodeGenerator _codeGenerator;

        public StudentController(ApplicationDbContext context, StudentCodeGenerator codeGenerator)
        {
            _context = context;
            _codeGenerator = codeGenerator;
        }

        // =========================
        // GET: Student?facultyCode=CNTT (Bài 11: trang khung, bảng được nạp bằng Ajax)
        // =========================
        public async Task<IActionResult> Index(string? facultyCode)
        {
            ViewBag.FacultyList = new SelectList(
                await _context.Faculties.OrderBy(f => f.Name).ToListAsync(),
                nameof(Faculty.FacultyCode), nameof(Faculty.Name), facultyCode);

            return View();
        }


        // =========================
        // GET: Student/List?facultyCode=CNTT&keyword=an (Ajax)
        // =========================
        public async Task<IActionResult> List(string? facultyCode, string? keyword)
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

            return PartialView("_Table", await query.OrderBy(s => s.StudentCode).ToListAsync());
        }


        // =========================
        // GET: Student/Details/2821050199 (Ajax)
        // =========================
        public async Task<IActionResult> Details(string id)
        {
            var student = await _context.Students
                .Include(s => s.Faculty)
                .Include(s => s.Major)
                .FirstOrDefaultAsync(s => s.StudentCode == id);

            if (student == null)
            {
                return NotFound();
            }

            return PartialView("_Details", student);
        }


        // =========================
        // GET: Student/Form hoặc Student/Form/2821050199 (Ajax)
        // =========================
        public async Task<IActionResult> Form(string? id)
        {
            Student? student;

            if (id == null)
            {
                student = new Student
                {
                    EnrollmentYear = DateTime.Now.Year,
                    TrainingSystemCode = "21",
                };
            }
            else
            {
                student = await _context.Students
                    .Include(s => s.Faculty)
                    .Include(s => s.Major)
                    .FirstOrDefaultAsync(s => s.StudentCode == id);

                if (student == null)
                {
                    return NotFound();
                }

                ViewData["IsEdit"] = true;
            }

            await LoadDropDownsAsync(student);

            return PartialView("_Form", student);
        }


        // =========================
        // POST: Student/Save (Ajax)
        // Thêm mới: không nhận StudentCode từ form, mã được sinh tự động ở server.
        // Sửa: năm nhập học, hệ, khoa, ngành đã nằm trong mã sinh viên nên chỉ cho sửa thông tin cá nhân
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(bool isEdit, string? studentCode,
            [Bind("FullName,DateOfBirth,Email,EnrollmentYear,TrainingSystemCode,FacultyCode,MajorCode")] Student input)
        {
            if (isEdit)
            {
                return await UpdateAsync(studentCode);
            }

            ModelState.Remove(nameof(Student.StudentCode));

            await ValidateCodesAsync(input);

            if (ModelState.IsValid)
            {
                // Thử lại nếu 2 người thêm cùng lúc và lấy trùng mã (lỗi trùng khoá chính)
                for (var lanThu = 1; lanThu <= 3; lanThu++)
                {
                    var code = await _codeGenerator.GenerateAsync(input.EnrollmentYear, input.TrainingSystemCode, input.MajorCode);

                    if (code == null)
                    {
                        ModelState.AddModelError(string.Empty, "Đã dùng hết số thứ tự (9999) cho năm, hệ và ngành này");
                        break;
                    }

                    input.StudentCode = code;
                    _context.Students.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        var maTiepTheo = await _codeGenerator.GenerateAsync(input.EnrollmentYear, input.TrainingSystemCode, input.MajorCode);

                        return JsonOk($"Đã thêm sinh viên {input.FullName}, mã sinh viên {input.StudentCode}. Sinh viên tiếp theo sẽ có mã {maTiepTheo}");
                    }
                    catch (DbUpdateException) when (lanThu < 3)
                    {
                        _context.Entry(input).State = EntityState.Detached;
                    }
                }
            }

            await LoadDropDownsAsync(input);

            return FormWithErrors(input);
        }

        private async Task<IActionResult> UpdateAsync(string? studentCode)
        {
            var student = await _context.Students
                .Include(s => s.Faculty)
                .Include(s => s.Major)
                .FirstOrDefaultAsync(s => s.StudentCode == studentCode);

            if (student == null)
            {
                return JsonFail("Sinh viên không còn tồn tại");
            }

            // Bỏ lỗi của các ô không được sửa (đã bị khoá trên form)
            foreach (var key in new[] { nameof(Student.EnrollmentYear), nameof(Student.TrainingSystemCode), nameof(Student.FacultyCode), nameof(Student.MajorCode), nameof(Student.StudentCode) })
            {
                ModelState.Remove(key);
            }

            if (await TryUpdateModelAsync(student, "", s => s.FullName, s => s.DateOfBirth, s => s.Email))
            {
                await _context.SaveChangesAsync();

                return JsonOk($"Đã cập nhật sinh viên {student.StudentCode}");
            }

            ViewData["IsEdit"] = true;

            await LoadDropDownsAsync(student);

            return FormWithErrors(student);
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
        // POST: Student/Delete/2821050199 (Ajax)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return JsonFail("Sinh viên không còn tồn tại");
            }

            _context.Students.Remove(student);

            await _context.SaveChangesAsync();

            return JsonOk($"Đã xoá sinh viên {student.StudentCode} - {student.FullName}");
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
