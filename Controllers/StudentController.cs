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

        private bool IsAjaxRequest() => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        public async Task<IActionResult> Index(string? facultyCode, string? keyword)
        {
            var query = _context.Students.Include(s => s.Faculty).Include(s => s.Major).AsQueryable();
            if (!string.IsNullOrEmpty(facultyCode)) query = query.Where(s => s.FacultyCode == facultyCode);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var term = keyword.Trim();
                query = query.Where(s => s.FullName.Contains(term) || s.StudentCode.Contains(term));
            }
            ViewBag.FacultyList = new SelectList(await _context.Faculties.OrderBy(f => f.Name).ToListAsync(),
                nameof(Faculty.FacultyCode), nameof(Faculty.Name), facultyCode);
            ViewBag.Keyword = keyword;
            return View(await query.OrderBy(s => s.StudentCode).ToListAsync());
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.Include(s => s.Faculty).Include(s => s.Major)
                .FirstOrDefaultAsync(s => s.StudentCode == id);
            if (student == null) return NotFound();
            return IsAjaxRequest() ? PartialView("_Details", student) : View(student);
        }

        public async Task<IActionResult> Create()
        {
            var student = new Student { EnrollmentYear = DateTime.Now.Year, TrainingSystemCode = "21" };
            await LoadDropDownsAsync(student);
            return IsAjaxRequest() ? PartialView("_Create", student) : View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,DateOfBirth,Email,EnrollmentYear,TrainingSystemCode,FacultyCode,MajorCode")] Student student)
        {
            ModelState.Remove(nameof(Student.StudentCode));
            await ValidateCodesAsync(student);
            if (ModelState.IsValid)
            {
                for (var attempt = 1; attempt <= 3; attempt++)
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
                        TempData["ThongBao"] = $"Đã thêm sinh viên {student.FullName}, mã {student.StudentCode}";
                        return IsAjaxRequest()
                            ? Json(new { success = true, message = $"Đã thêm sinh viên {student.FullName}, mã {student.StudentCode}" })
                            : RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateException) when (attempt < 3)
                    {
                        _context.Entry(student).State = EntityState.Detached;
                    }
                    catch (DbUpdateException)
                    {
                        _context.Entry(student).State = EntityState.Detached;
                        ModelState.AddModelError(string.Empty, "Không thể lưu sinh viên. Vui lòng thử lại.");
                    }
                }
            }
            await LoadDropDownsAsync(student);
            return IsAjaxRequest() ? PartialView("_Create", student) : View(student);
        }

        public async Task<IActionResult> NextCode(int enrollmentYear, string? trainingSystemCode, string? majorCode)
        {
            if (enrollmentYear < 2000 || enrollmentYear > 2099 || !TrainingSystems.IsValid(trainingSystemCode)
                || majorCode == null || !await _context.Majors.AnyAsync(m => m.MajorCode == majorCode))
                return Json(new { code = (string?)null });
            return Json(new { code = await _codeGenerator.GenerateAsync(enrollmentYear, trainingSystemCode!, majorCode) });
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.Include(s => s.Faculty).Include(s => s.Major)
                .FirstOrDefaultAsync(s => s.StudentCode == id);
            if (student == null) return NotFound();
            return IsAjaxRequest() ? PartialView("_Edit", student) : View(student);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(string? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.Include(s => s.Faculty).Include(s => s.Major)
                .FirstOrDefaultAsync(s => s.StudentCode == id);
            if (student == null) return NotFound();

            await TryUpdateModelAsync(student, "", s => s.FullName, s => s.DateOfBirth, s => s.Email);
            if (!ModelState.IsValid)
                return IsAjaxRequest() ? PartialView("_Edit", student) : View(student);

            await _context.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã cập nhật sinh viên {student.StudentCode}";
            return IsAjaxRequest()
                ? Json(new { success = true, message = $"Đã cập nhật sinh viên {student.StudentCode}" })
                : RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.Include(s => s.Faculty).Include(s => s.Major)
                .FirstOrDefaultAsync(s => s.StudentCode == id);
            if (student == null) return NotFound();
            return IsAjaxRequest() ? PartialView("_Delete", student) : View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã xoá sinh viên {student.StudentCode} - {student.FullName}";
            return IsAjaxRequest()
                ? Json(new { success = true, message = $"Đã xoá sinh viên {student.StudentCode}" })
                : RedirectToAction(nameof(Index));
        }

        private async Task ValidateCodesAsync(Student student)
        {
            if (!string.IsNullOrEmpty(student.TrainingSystemCode) && !TrainingSystems.IsValid(student.TrainingSystemCode))
                ModelState.AddModelError(nameof(Student.TrainingSystemCode), "Hệ đào tạo không hợp lệ");
            if (!string.IsNullOrEmpty(student.MajorCode))
            {
                var major = await _context.Majors.FindAsync(student.MajorCode);
                if (major == null) ModelState.AddModelError(nameof(Student.MajorCode), "Ngành không tồn tại");
                else if (major.FacultyCode != student.FacultyCode)
                    ModelState.AddModelError(nameof(Student.MajorCode), "Ngành đã chọn không thuộc khoa đã chọn");
            }
        }

        private async Task LoadDropDownsAsync(Student student)
        {
            ViewBag.TrainingSystemList = new SelectList(
                TrainingSystems.All.Select(t => new { Code = t.Key, Name = $"{t.Key} - {t.Value}" }),
                "Code", "Name", student.TrainingSystemCode);
            ViewBag.FacultyList = new SelectList(
                await _context.Faculties.Where(f => f.Majors.Any()).OrderBy(f => f.Name).ToListAsync(),
                nameof(Faculty.FacultyCode), nameof(Faculty.Name), student.FacultyCode);
            ViewBag.Majors = await _context.Majors.OrderBy(m => m.MajorCode)
                .Select(m => new { code = m.MajorCode, name = m.Name, facultyCode = m.FacultyCode }).ToListAsync();
        }
    }
}
