using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoMVC.Data;
using DemoMVC.Helpers;
using DemoMVC.Models;

namespace DemoMVC.Controllers
{
    // Bài 11: CRUD khoa bằng Modal + Ajax. Khoa còn ngành hoặc sinh viên thì không xoá được (Restrict)
    public class FacultyController : AjaxCrudController
    {
        private readonly ApplicationDbContext _context;

        public FacultyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET: Faculty
        // =========================
        public IActionResult Index()
        {
            return View();
        }


        // =========================
        // GET: Faculty/List (Ajax)
        // =========================
        public async Task<IActionResult> List(string? keyword, int page = 1, int pageSize = PaginatedList<object>.DefaultPageSize)
        {
            var faculties = _context.Faculties
                .Include(f => f.Majors)
                .Include(f => f.Students)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var tuKhoa = keyword.Trim();
                faculties = faculties.Where(f => f.FacultyCode.Contains(tuKhoa) || f.Name.Contains(tuKhoa));
            }

            var model = await PaginatedList<Faculty>.CreateAsync(faculties.OrderBy(f => f.Name), page, pageSize, keyword);
            return PartialView("_Table", model);
        }


        // =========================
        // GET: Faculty/Details/CNTT (Ajax)
        // =========================
        public async Task<IActionResult> Details(string id)
        {
            var faculty = await _context.Faculties
                .Include(f => f.Majors)
                .Include(f => f.Students)
                .FirstOrDefaultAsync(f => f.FacultyCode == id);

            if (faculty == null)
            {
                return NotFound();
            }

            return PartialView("_Details", faculty);
        }


        // =========================
        // GET: Faculty/Form hoặc Faculty/Form/CNTT (Ajax)
        // =========================
        public async Task<IActionResult> Form(string? id)
        {
            if (id == null)
            {
                return PartialView("_Form", new Faculty());
            }

            var faculty = await _context.Faculties.FindAsync(id);

            if (faculty == null)
            {
                return NotFound();
            }

            ViewData["IsEdit"] = true;

            return PartialView("_Form", faculty);
        }


        // =========================
        // POST: Faculty/Save (Ajax)
        // isEdit gửi từ ô ẩn: mã khoa là khoá chính nên khi sửa chỉ đổi được tên
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(bool isEdit, [Bind("FacultyCode,Name")] Faculty input)
        {
            var faculty = ModelState.IsValid ? await _context.Faculties.FindAsync(input.FacultyCode) : null;

            if (!isEdit && faculty != null)
            {
                ModelState.AddModelError(nameof(Faculty.FacultyCode), "Mã khoa đã tồn tại");
            }

            if (!ModelState.IsValid)
            {
                ViewData["IsEdit"] = isEdit;

                return FormWithErrors(input);
            }

            if (!isEdit)
            {
                _context.Faculties.Add(input);

                await _context.SaveChangesAsync();

                return JsonOk($"Đã thêm {input.Name}");
            }

            if (faculty == null)
            {
                return JsonFail("Khoa không còn tồn tại");
            }

            faculty.Name = input.Name;

            await _context.SaveChangesAsync();

            return JsonOk($"Đã cập nhật {faculty.Name}");
        }


        // =========================
        // POST: Faculty/Delete/CNTT (Ajax)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var faculty = await _context.Faculties.FindAsync(id);

            if (faculty == null)
            {
                return JsonFail("Khoa không còn tồn tại");
            }

            if (await _context.Majors.AnyAsync(m => m.FacultyCode == id)
                || await _context.Students.AnyAsync(s => s.FacultyCode == id))
            {
                return JsonFail($"Không xoá được {faculty.Name} vì vẫn còn ngành hoặc sinh viên thuộc khoa");
            }

            _context.Faculties.Remove(faculty);

            await _context.SaveChangesAsync();

            return JsonOk($"Đã xoá {faculty.Name}");
        }
    }
}
