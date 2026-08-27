using Microsoft.AspNetCore.Mvc;

namespace DemoMVC.Controllers;

// [Route] đặt ở controller làm tiền tố chung cho mọi action
[Route("cua-hang")]
public class CuaHangController : Controller
{
    // GET /cua-hang
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewBag.GiaiThich = "Tiền tố [Route(\"cua-hang\")] + [HttpGet(\"\")].";
        return View("~/Views/DinhTuyen/KetQua.cshtml");
    }

    // GET /cua-hang/danh-muc/dien-thoai
    [HttpGet("danh-muc/{ten}")]
    public IActionResult DanhMuc(string ten)
    {
        ViewBag.GiaiThich = $"Tiền tố \"cua-hang\" + \"danh-muc/{{ten}}\" với ten = \"{ten}\".";
        return View("~/Views/DinhTuyen/KetQua.cshtml");
    }

    // Token [action] được thay bằng tên action: GET /cua-hang/LienHe
    [HttpGet("[action]")]
    public IActionResult LienHe()
    {
        ViewBag.GiaiThich = "Token [action] được thay bằng tên action \"LienHe\".";
        return View("~/Views/DinhTuyen/KetQua.cshtml");
    }
}
