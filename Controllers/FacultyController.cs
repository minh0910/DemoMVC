using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoMVC.Data;
using DemoMVC.Models;

namespace DemoMVC.Controllers
{
    public class FacultyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FacultyController(ApplicationDbContext context) => _context = context;

        private bool IsAjaxRequest() => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        public async Task<IActionResult> Index()
        {
            var faculties = await _context.Faculties
                .Include(f => f.Majors)
                .OrderBy(f => f.Name)
                .ToListAsync();
            return View(faculties);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();
            var faculty = await _context.Faculties
                .Include(f => f.Majors)
                .Include(f => f.Students).ThenInclude(s => s.Major)
                .AsSplitQuery()
                .FirstOrDefaultAsync(f => f.FacultyCode == id);
            if (faculty == null) return NotFound();
            return IsAjaxRequest() ? PartialView("_Details", faculty) : View(faculty);
        }

        public IActionResult Create()
        {
            var faculty = new Faculty();
            return IsAjaxRequest() ? PartialView("_Create", faculty) : View(faculty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FacultyCode,Name")] Faculty faculty)
        {
            faculty.FacultyCode = faculty.FacultyCode?.Trim().ToUpperInvariant() ?? string.Empty;
            if (ModelState.IsValid && await _context.Faculties.AnyAsync(f => f.FacultyCode == faculty.FacultyCode))
                ModelState.AddModelError(nameof(Faculty.FacultyCode), "Mã khoa đã tồn tại");

            if (!ModelState.IsValid)
                return IsAjaxRequest() ? PartialView("_Create", faculty) : View(faculty);

            _context.Faculties.Add(faculty);
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã thêm {faculty.Name}";
            return IsAjaxRequest()
                ? Json(new { success = true, message = $"Đã thêm {faculty.Name}" })
                : RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null) return NotFound();
            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null) return NotFound();
            return IsAjaxRequest() ? PartialView("_Edit", faculty) : View(faculty);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(string? id)
        {
            if (id == null) return NotFound();
            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null) return NotFound();

            await TryUpdateModelAsync(faculty, "", f => f.Name);
            if (!ModelState.IsValid)
                return IsAjaxRequest() ? PartialView("_Edit", faculty) : View(faculty);

            await _context.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã cập nhật {faculty.Name}";
            return IsAjaxRequest()
                ? Json(new { success = true, message = $"Đã cập nhật {faculty.Name}" })
                : RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null) return NotFound();
            var faculty = await _context.Faculties.FirstOrDefaultAsync(f => f.FacultyCode == id);
            if (faculty == null) return NotFound();
            ViewBag.MajorCount = await _context.Majors.CountAsync(m => m.FacultyCode == id);
            ViewBag.StudentCount = await _context.Students.CountAsync(s => s.FacultyCode == id);
            return IsAjaxRequest() ? PartialView("_Delete", faculty) : View(faculty);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null) return NotFound();

            if (await _context.Majors.AnyAsync(m => m.FacultyCode == id)
                || await _context.Students.AnyAsync(s => s.FacultyCode == id))
            {
                const string error = "Không xoá được vì vẫn còn ngành hoặc sinh viên thuộc khoa.";
                if (IsAjaxRequest()) return Json(new { success = false, message = error });
                TempData["Loi"] = error;
                return RedirectToAction(nameof(Index));
            }

            _context.Faculties.Remove(faculty);
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã xoá {faculty.Name}";
            return IsAjaxRequest()
                ? Json(new { success = true, message = $"Đã xoá {faculty.Name}" })
                : RedirectToAction(nameof(Index));
        }
    }
}
