using Microsoft.AspNetCore.Mvc;

namespace DemoMVC.Controllers;

public class CicdController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
