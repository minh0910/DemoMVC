using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DemoMVC.Data;
using DemoMVC.Models;

namespace DemoMVC.Controllers
{
    public class MajorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MajorController(ApplicationDbContext context) => _context = context;

        private bool IsAjaxRequest() => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        public async Task<IActionResult> Index()
        {
            var majors = await _context.Majors.Include(m => m.Faculty)
                .OrderBy(m => m.MajorCode).ToListAsync();
            return View(majors);
        }

        public async Task<IActionResult> Create()
        {
            await LoadFacultyListAsync(null);
            var major = new Major();
            return IsAjaxRequest() ? PartialView("_Create", major) : View(major);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MajorCode,Name,FacultyCode")] Major major)
        {
            major.MajorCode = major.MajorCode?.Trim() ?? string.Empty;
            if (ModelState.IsValid && await _context.Majors.AnyAsync(m => m.MajorCode == major.MajorCode))
                ModelState.AddModelError(nameof(Major.MajorCode), "Mã ngành đã tồn tại");
            if (ModelState.IsValid && !await _context.Faculties.AnyAsync(f => f.FacultyCode == major.FacultyCode))
                ModelState.AddModelError(nameof(Major.FacultyCode), "Khoa không tồn tại");

            if (!ModelState.IsValid)
            {
                await LoadFacultyListAsync(major.FacultyCode);
                return IsAjaxRequest() ? PartialView("_Create", major) : View(major);
            }

            _context.Majors.Add(major);
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã thêm ngành {major.MajorCode} - {major.Name}";
            return IsAjaxRequest()
                ? Json(new { success = true, message = $"Đã thêm ngành {major.Name}" })
                : RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null) return NotFound();
            var major = await _context.Majors.FindAsync(id);
            if (major == null) return NotFound();
            await LoadFacultyListAsync(major.FacultyCode);
            return IsAjaxRequest() ? PartialView("_Edit", major) : View(major);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(string? id)
        {
            if (id == null) return NotFound();
            var major = await _context.Majors.FindAsync(id);
            if (major == null) return NotFound();
            var oldFacultyCode = major.FacultyCode;

            await TryUpdateModelAsync(major, "", m => m.Name, m => m.FacultyCode);
            if (ModelState.IsValid && !await _context.Faculties.AnyAsync(f => f.FacultyCode == major.FacultyCode))
                ModelState.AddModelError(nameof(Major.FacultyCode), "Khoa không tồn tại");
            else if (ModelState.IsValid && major.FacultyCode != oldFacultyCode
                && await _context.Students.AnyAsync(s => s.MajorCode == id))
                ModelState.AddModelError(nameof(Major.FacultyCode), "Ngành đã có sinh viên nên không đổi được khoa");

            if (!ModelState.IsValid)
            {
                await LoadFacultyListAsync(major.FacultyCode);
                return IsAjaxRequest() ? PartialView("_Edit", major) : View(major);
            }

            await _context.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã cập nhật ngành {major.MajorCode} - {major.Name}";
            return IsAjaxRequest()
                ? Json(new { success = true, message = $"Đã cập nhật ngành {major.Name}" })
                : RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null) return NotFound();
            var major = await _context.Majors.Include(m => m.Faculty)
                .FirstOrDefaultAsync(m => m.MajorCode == id);
            if (major == null) return NotFound();
            ViewBag.StudentCount = await _context.Students.CountAsync(s => s.MajorCode == id);
            return IsAjaxRequest() ? PartialView("_Delete", major) : View(major);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var major = await _context.Majors.FindAsync(id);
            if (major == null) return NotFound();
            if (await _context.Students.AnyAsync(s => s.MajorCode == id))
            {
                const string error = "Không xoá được ngành vì vẫn còn sinh viên thuộc ngành.";
                if (IsAjaxRequest()) return Json(new { success = false, message = error });
                TempData["Loi"] = error;
                return RedirectToAction(nameof(Index));
            }

            _context.Majors.Remove(major);
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã xoá ngành {major.Name}";
            return IsAjaxRequest()
                ? Json(new { success = true, message = $"Đã xoá ngành {major.Name}" })
                : RedirectToAction(nameof(Index));
        }

        private async Task LoadFacultyListAsync(string? selectedFacultyCode)
        {
            var faculties = await _context.Faculties.OrderBy(f => f.Name).ToListAsync();
            ViewBag.FacultyList = new SelectList(faculties, nameof(Faculty.FacultyCode), nameof(Faculty.Name), selectedFacultyCode);
        }
    }
}
