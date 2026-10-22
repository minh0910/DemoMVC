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

        public MajorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET: Major
        // =========================
        public async Task<IActionResult> Index()
        {
            var majors = await _context.Majors
                .Include(m => m.Faculty)         // JOIN sang bảng Khoa để lấy tên khoa
                .OrderBy(m => m.MajorCode)
                .ToListAsync();

            return View(majors);
        }


        // =========================
        // GET: Major/Create
        // =========================
        public async Task<IActionResult> Create()
        {
            await LoadFacultyListAsync(null);

            return View();
        }


        // =========================
        // POST: Major/Create
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MajorCode,Name,FacultyCode")] Major major)
        {
            if (ModelState.IsValid && await _context.Majors.AnyAsync(m => m.MajorCode == major.MajorCode))
            {
                ModelState.AddModelError(nameof(Major.MajorCode), "Mã ngành đã tồn tại");
            }

            if (ModelState.IsValid && !await _context.Faculties.AnyAsync(f => f.FacultyCode == major.FacultyCode))
            {
                ModelState.AddModelError(nameof(Major.FacultyCode), "Khoa không tồn tại");
            }

            if (ModelState.IsValid)
            {
                _context.Majors.Add(major);

                await _context.SaveChangesAsync();

                TempData["ThongBao"] = $"Đã thêm ngành {major.MajorCode} - {major.Name}";

                return RedirectToAction(nameof(Index));
            }

            await LoadFacultyListAsync(major.FacultyCode);

            return View(major);
        }


        // =========================
        // GET: Major/Edit/05
        // =========================
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var major = await _context.Majors.FindAsync(id);

            if (major == null)
            {
                return NotFound();
            }

            await LoadFacultyListAsync(major.FacultyCode);

            return View(major);
        }


        // =========================
        // POST: Major/Edit/05
        // Mã ngành là khoá chính và nằm trong mã sinh viên nên không cho sửa
        // =========================
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var major = await _context.Majors.FindAsync(id);

            if (major == null)
            {
                return NotFound();
            }

            var khoaCu = major.FacultyCode;

            if (await TryUpdateModelAsync(major, "", m => m.Name, m => m.FacultyCode))
            {
                if (!await _context.Faculties.AnyAsync(f => f.FacultyCode == major.FacultyCode))
                {
                    ModelState.AddModelError(nameof(Major.FacultyCode), "Khoa không tồn tại");
                }
                else if (major.FacultyCode != khoaCu && await _context.Students.AnyAsync(s => s.MajorCode == id))
                {
                    // Sinh viên lưu cả mã khoa và mã ngành: đổi khoa của ngành sẽ làm dữ liệu không khớp
                    ModelState.AddModelError(nameof(Major.FacultyCode), "Ngành đã có sinh viên nên không đổi được khoa");
                }
            }

            if (ModelState.IsValid)
            {
                await _context.SaveChangesAsync();

                TempData["ThongBao"] = $"Đã cập nhật ngành {major.MajorCode} - {major.Name}";

                return RedirectToAction(nameof(Index));
            }

            await LoadFacultyListAsync(major.FacultyCode);

            return View(major);
        }


        // =========================
        // GET: Major/Delete/05
        // =========================
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var major = await _context.Majors
                .Include(m => m.Faculty)
                .FirstOrDefaultAsync(m => m.MajorCode == id);

            if (major == null)
            {
                return NotFound();
            }

            ViewBag.StudentCount = await _context.Students.CountAsync(s => s.MajorCode == id);

            return View(major);
        }


        // =========================
        // POST: Major/Delete/05
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var major = await _context.Majors.FindAsync(id);

            if (major != null)
            {
                if (await _context.Students.AnyAsync(s => s.MajorCode == id))
                {
                    TempData["Loi"] = $"Không xoá được ngành {major.Name} vì vẫn còn sinh viên thuộc ngành";

                    return RedirectToAction(nameof(Index));
                }

                _context.Majors.Remove(major);

                await _context.SaveChangesAsync();

                TempData["ThongBao"] = $"Đã xoá ngành {major.Name}";
            }

            return RedirectToAction(nameof(Index));
        }


        // Danh sách khoa cho dropdown chọn khoá ngoại
        private async Task LoadFacultyListAsync(string? selectedFacultyCode)
        {
            var faculties = await _context.Faculties.OrderBy(f => f.Name).ToListAsync();

            ViewBag.FacultyList = new SelectList(faculties, nameof(Faculty.FacultyCode), nameof(Faculty.Name), selectedFacultyCode);
        }
    }
}
