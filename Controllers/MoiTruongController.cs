using Microsoft.AspNetCore.Mvc;

namespace DemoMVC.Controllers;

public class MoiTruongController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
