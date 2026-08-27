using Microsoft.AspNetCore.Mvc;

namespace DemoMVC.Controllers;

public class DinhTuyenController : Controller
{
    // Route mặc định (conventional): /DinhTuyen hoặc /DinhTuyen/Index
    public IActionResult Index()
    {
        return View();
    }

    // Route mặc định: /DinhTuyen/ChiTiet/5?tab=mo-ta
    // id lấy từ segment {id?}, tab lấy từ query string
    public IActionResult ChiTiet(int? id, string? tab)
    {
        ViewBag.GiaiThich = "Khớp route mặc định {controller=Home}/{action=Index}/{id?}. "
            + $"id = {(id?.ToString() ?? "null")}, tab = {tab ?? "null"}.";
        return View("KetQua");
    }

    // Attribute routing: URL thân thiện, có ràng buộc id phải là số nguyên
    [Route("san-pham/{id:int}")]
    public IActionResult SanPham(int id)
    {
        ViewBag.GiaiThich = $"Khớp [Route(\"san-pham/{{id:int}}\")] với id = {id}.";
        return View("KetQua");
    }

    // Nhiều tham số + ràng buộc min
    [Route("tin-tuc/{nam:int:min(2000)}/{slug}")]
    public IActionResult TinTuc(int nam, string slug)
    {
        ViewBag.GiaiThich = $"Bài viết năm {nam}, slug = \"{slug}\".";
        return View("KetQua");
    }

    // Một action có nhiều route
    [Route("gioi-thieu")]
    [Route("about")]
    public IActionResult GioiThieu()
    {
        ViewBag.GiaiThich = "Action GioiThieu có 2 route: /gioi-thieu và /about.";
        return View("KetQua");
    }

    // Tham số có giá trị mặc định: /trang và /trang/3 đều khớp
    [Route("trang/{so:int=1}")]
    public IActionResult Trang(int so)
    {
        ViewBag.GiaiThich = $"Đang ở trang số {so} (không truyền thì mặc định là 1).";
        return View("KetQua");
    }

    // Chỉ chấp nhận POST -> truy cập bằng GET sẽ bị lỗi 405
    [HttpPost("dinh-tuyen/chi-post")]
    public IActionResult ChiPost()
    {
        ViewBag.GiaiThich = "Request POST đã khớp [HttpPost(\"dinh-tuyen/chi-post\")].";
        return View("KetQua");
    }

    // Route khớp nhưng không có file View -> lỗi 500 InvalidOperationException
    public IActionResult ThieuView()
    {
        return View();
    }
}
