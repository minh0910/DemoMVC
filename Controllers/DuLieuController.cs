using Microsoft.AspNetCore.Mvc;
using DemoMVC.Models;

namespace DemoMVC.Controllers;

public class DuLieuController : Controller
{
    // Dữ liệu mẫu lưu trong bộ nhớ (mất khi khởi động lại ứng dụng)
    private static readonly List<SinhVien> _dsSinhVien =
    [
        new SinhVien { MaSV = "SV001", HoTen = "Nguyễn Văn An", Tuoi = 20, Email = "an@example.com", Khoa = "CNTT" },
        new SinhVien { MaSV = "SV002", HoTen = "Trần Thị Bình", Tuoi = 21, Email = "binh@example.com", Khoa = "Kinh tế" },
        new SinhVien { MaSV = "SV003", HoTen = "Lê Minh Cường", Tuoi = 19, Khoa = "Ngoại ngữ" },
    ];
    private static readonly Lock _khoa = new();

    public IActionResult Index()
    {
        return View();
    }

    // ===== 1. Controller -> View (không dùng Model) =====
    public IActionResult ControllerSangView()
    {
        ViewBag.TieuDe = "Xin chào từ Controller!";
        ViewData["ThoiGian"] = DateTime.Now;
        ViewBag.DsMonHoc = new List<string> { "Lập trình C#", "ASP.NET Core MVC", "Cơ sở dữ liệu" };
        return View();
    }

    // ===== 2. View -> Controller (không dùng Model) =====
    // GET: hiển thị form; nếu có query string ?tuKhoa=... thì tìm kiếm
    [HttpGet]
    public IActionResult ViewSangController(string? tuKhoa)
    {
        if (tuKhoa != null)
        {
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.KetQuaTim = _dsSinhVien
                .Where(s => s.HoTen.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.HoTen)
                .ToList();
        }
        return View();
    }

    // POST: nhận dữ liệu form qua tham số (tên tham số trùng thuộc tính name của input)
    [HttpPost]
    public IActionResult ViewSangController(string hoTen, int namSinh, IFormCollection form)
    {
        ViewBag.HoTen = hoTen;
        ViewBag.NamSinh = namSinh;
        ViewBag.Tuoi = DateTime.Now.Year - namSinh;
        ViewBag.GioiTinh = form["gioiTinh"]; // cách khác: đọc từ IFormCollection
        return View();
    }

    // ===== 3. Controller <-> View thông qua Model =====
    // Truyền 1 đối tượng
    public IActionResult ThongTin(string id = "SV001")
    {
        SinhVien? sv;
        lock (_khoa) sv = _dsSinhVien.FirstOrDefault(s => s.MaSV == id);
        if (sv == null) return NotFound();
        return View(sv);
    }

    // Truyền danh sách đối tượng
    public IActionResult DanhSach()
    {
        List<SinhVien> ds;
        lock (_khoa) ds = _dsSinhVien.ToList();
        return View(ds);
    }

    [HttpGet]
    public IActionResult ThemMoi()
    {
        return View(new SinhVien());
    }

    // Model binding: dữ liệu form tự gán vào đối tượng SinhVien
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ThemMoi(SinhVien sv)
    {
        lock (_khoa)
        {
            if (_dsSinhVien.Any(s => s.MaSV.Equals(sv.MaSV, StringComparison.OrdinalIgnoreCase)))
                ModelState.AddModelError(nameof(SinhVien.MaSV), "Mã sinh viên đã tồn tại");

            if (!ModelState.IsValid)
                return View(sv); // trả lại form kèm dữ liệu đã nhập và thông báo lỗi

            _dsSinhVien.Add(sv);
        }
        return RedirectToAction(nameof(DanhSach));
    }
}
