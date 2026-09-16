using Microsoft.AspNetCore.Mvc;

namespace DemoMVC.Controllers;

public class TrangThaiController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    // ===== ViewBag & ViewData: chỉ tồn tại trong 1 request =====
    public IActionResult ViewBagViewData()
    {
        ViewBag.HoTen = "Nguyễn Văn An";                        // ViewBag: dynamic
        ViewData["Lop"] = "K66-CNTT";                           // ViewData: Dictionary<string, object?>
        ViewData["DiemTB"] = 8.25;
        ViewBag.MonHoc = new[] { "C#", "ASP.NET Core", "SQL" };

        // ViewBag và ViewData dùng chung một nơi lưu trữ
        ViewBag.ChungKho = "Gán bằng ViewBag, đọc bằng ViewData";
        ViewData["ChungKho2"] = "Gán bằng ViewData, đọc bằng ViewBag";
        return View();
    }

    // ===== TempData: tồn tại qua redirect, đọc xong thì bị xoá =====
    // cheDo: "doc" (đọc thường), "peek" (đọc không xoá), "keep" (đọc rồi giữ lại)
    public IActionResult GuiTempData(string cheDo = "doc")
    {
        TempData["ThongBao"] = $"TempData được gán lúc {DateTime.Now:HH:mm:ss}";
        ViewBag.ThongBao = "ViewBag được gán trước khi redirect";
        return RedirectToAction(nameof(NhanTempData), new { cheDo });
    }

    public IActionResult NhanTempData(string cheDo = "doc")
    {
        ViewBag.CheDo = cheDo;
        ViewBag.TempDataDocDuoc = cheDo switch
        {
            "peek" => TempData.Peek("ThongBao"), // đọc nhưng không đánh dấu xoá
            _ => TempData["ThongBao"]            // đọc và đánh dấu xoá sau request
        };
        if (cheDo == "keep")
        {
            TempData.Keep("ThongBao");           // huỷ đánh dấu xoá
        }
        return View();
    }
}
