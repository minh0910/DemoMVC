using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DemoMVC.Data;
using DemoMVC.Models;

namespace DemoMVC.Controllers
{
    // Bài 11: CRUD ngành học bằng Modal + Ajax. Ngành thuộc 1 khoa (khoá ngoại FacultyCode)
    public class MajorController : AjaxCrudController
    {
        private readonly ApplicationDbContext _context;

        public MajorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET: Major
        // =========================
        public IActionResult Index()
        {
            return View();
        }


        // =========================
        // GET: Major/List (Ajax)
        // =========================
        public async Task<IActionResult> List()
        {
            var majors = await _context.Majors
                .Include(m => m.Faculty)
                .Include(m => m.Students)
                .OrderBy(m => m.MajorCode)
                .ToListAsync();

            return PartialView("_Table", majors);
        }


        // =========================
        // GET: Major/Form hoặc Major/Form/05 (Ajax)
        // =========================
        public async Task<IActionResult> Form(string? id)
        {
            var major = id == null ? new Major() : await _context.Majors.FindAsync(id);

            if (major == null)
            {
                return NotFound();
            }

            ViewData["IsEdit"] = id != null;

            await LoadFacultyListAsync(major.FacultyCode);

            return PartialView("_Form", major);
        }


        // =========================
        // POST: Major/Save (Ajax)
        // Mã ngành là khoá chính và nằm trong mã sinh viên nên khi sửa không đổi được mã
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(bool isEdit, [Bind("MajorCode,Name,FacultyCode")] Major input)
        {
            var major = ModelState.IsValid ? await _context.Majors.FindAsync(input.MajorCode) : null;

            if (!isEdit && major != null)
            {
                ModelState.AddModelError(nameof(Major.MajorCode), "Mã ngành đã tồn tại");
            }

            if (ModelState.IsValid && !await _context.Faculties.AnyAsync(f => f.FacultyCode == input.FacultyCode))
            {
                ModelState.AddModelError(nameof(Major.FacultyCode), "Khoa không tồn tại");
            }

            // Sinh viên lưu cả mã khoa và mã ngành: đổi khoa của ngành đã có sinh viên sẽ làm dữ liệu không khớp
            if (ModelState.IsValid && isEdit && major != null && major.FacultyCode != input.FacultyCode
                && await _context.Students.AnyAsync(s => s.MajorCode == input.MajorCode))
            {
                ModelState.AddModelError(nameof(Major.FacultyCode), "Ngành đã có sinh viên nên không đổi được khoa");
            }

            if (!ModelState.IsValid)
            {
                ViewData["IsEdit"] = isEdit;

                await LoadFacultyListAsync(input.FacultyCode);

                return FormWithErrors(input);
            }

            if (!isEdit)
            {
                _context.Majors.Add(input);

                await _context.SaveChangesAsync();

                return JsonOk($"Đã thêm ngành {input.MajorCode} - {input.Name}");
            }

            if (major == null)
            {
                return JsonFail("Ngành không còn tồn tại");
            }

            major.Name = input.Name;
            major.FacultyCode = input.FacultyCode;

            await _context.SaveChangesAsync();

            return JsonOk($"Đã cập nhật ngành {major.MajorCode} - {major.Name}");
        }


        // =========================
        // POST: Major/Delete/05 (Ajax)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var major = await _context.Majors.FindAsync(id);

            if (major == null)
            {
                return JsonFail("Ngành không còn tồn tại");
            }

            if (await _context.Students.AnyAsync(s => s.MajorCode == id))
            {
                return JsonFail($"Không xoá được ngành {major.Name} vì vẫn còn sinh viên thuộc ngành");
            }

            _context.Majors.Remove(major);

            await _context.SaveChangesAsync();

            return JsonOk($"Đã xoá ngành {major.Name}");
        }


        private async Task LoadFacultyListAsync(string? selected)
        {
            ViewBag.FacultyList = new SelectList(
                await _context.Faculties.OrderBy(f => f.Name).ToListAsync(),
                nameof(Faculty.FacultyCode), nameof(Faculty.Name), selected);
        }
    }
}
