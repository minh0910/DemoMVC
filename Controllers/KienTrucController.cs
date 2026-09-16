using Microsoft.AspNetCore.Mvc;

namespace DemoMVC.Controllers;

public class KienTrucController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
