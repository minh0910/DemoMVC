using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoMVC.Data;
using DemoMVC.Models;

namespace DemoMVC.Controllers
{
    public class FacultyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FacultyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET: Faculty
        // =========================
        public async Task<IActionResult> Index()
        {
            var faculties = await _context.Faculties
                .Include(f => f.Majors)          // lấy kèm các ngành của khoa qua khoá ngoại
                .OrderBy(f => f.Name)
                .ToListAsync();

            return View(faculties);
        }


        // =========================
        // GET: Faculty/Details/CNTT
        // =========================
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faculty = await _context.Faculties
                .Include(f => f.Majors)
                .Include(f => f.Students)
                    .ThenInclude(s => s.Major)
                .AsSplitQuery()
                .FirstOrDefaultAsync(f => f.FacultyCode == id);

            if (faculty == null)
            {
                return NotFound();
            }

            return View(faculty);
        }


        // =========================
        // GET: Faculty/Create
        // =========================
        public IActionResult Create()
        {
            return View();
        }


        // =========================
        // POST: Faculty/Create
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FacultyCode,Name")] Faculty faculty)
        {
            faculty.FacultyCode = faculty.FacultyCode?.Trim().ToUpperInvariant() ?? string.Empty;

            if (ModelState.IsValid && await _context.Faculties.AnyAsync(f => f.FacultyCode == faculty.FacultyCode))
            {
                ModelState.AddModelError(nameof(Faculty.FacultyCode), "Mã khoa đã tồn tại");
            }

            if (ModelState.IsValid)
            {
                _context.Faculties.Add(faculty);

                await _context.SaveChangesAsync();

                TempData["ThongBao"] = $"Đã thêm {faculty.Name}";

                return RedirectToAction(nameof(Index));
            }

            return View(faculty);
        }


        // =========================
        // GET: Faculty/Edit/CNTT
        // =========================
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faculty = await _context.Faculties.FindAsync(id);

            if (faculty == null)
            {
                return NotFound();
            }

            return View(faculty);
        }


        // =========================
        // POST: Faculty/Edit/CNTT
        // Mã khoa là khoá chính (ngành, sinh viên đang tham chiếu tới) nên chỉ cho sửa tên
        // =========================
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faculty = await _context.Faculties.FindAsync(id);

            if (faculty == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync(faculty, "", f => f.Name))
            {
                await _context.SaveChangesAsync();

                TempData["ThongBao"] = $"Đã cập nhật {faculty.Name}";

                return RedirectToAction(nameof(Index));
            }

            return View(faculty);
        }


        // =========================
        // GET: Faculty/Delete/CNTT
        // =========================
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faculty = await _context.Faculties
                .FirstOrDefaultAsync(f => f.FacultyCode == id);

            if (faculty == null)
            {
                return NotFound();
            }

            ViewBag.MajorCount = await _context.Majors.CountAsync(m => m.FacultyCode == id);
            ViewBag.StudentCount = await _context.Students.CountAsync(s => s.FacultyCode == id);

            return View(faculty);
        }


        // =========================
        // POST: Faculty/Delete/CNTT
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var faculty = await _context.Faculties.FindAsync(id);

            if (faculty != null)
            {
                // Khoá ngoại để Restrict: còn ngành hoặc sinh viên thì không xoá được
                var dangDuocDung = await _context.Majors.AnyAsync(m => m.FacultyCode == id)
                    || await _context.Students.AnyAsync(s => s.FacultyCode == id);

                if (dangDuocDung)
                {
                    TempData["Loi"] = $"Không xoá được {faculty.Name} vì vẫn còn ngành hoặc sinh viên thuộc khoa";

                    return RedirectToAction(nameof(Index));
                }

                _context.Faculties.Remove(faculty);

                await _context.SaveChangesAsync();

                TempData["ThongBao"] = $"Đã xoá {faculty.Name}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
