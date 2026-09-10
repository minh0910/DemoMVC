using Microsoft.AspNetCore.Mvc;

namespace DemoMVC.Controllers;

public class BoCucController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    // Trang dùng các thành phần Bootstrap 5, nạp thêm file CSS riêng qua section Styles
    public IActionResult ViDuBootstrap()
    {
        return View();
    }

    // Trang chọn layout khác (_LayoutDemo) thay cho _Layout mặc định
    public IActionResult LayoutRieng()
    {
        return View();
    }
}
