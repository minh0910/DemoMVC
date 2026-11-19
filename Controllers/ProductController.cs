using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoMVC.Data;
using DemoMVC.Models;

namespace DemoMVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context) => _context = context;

        private bool IsAjaxRequest() =>
            Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        public async Task<IActionResult> Index() =>
            View(await _context.Products.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return IsAjaxRequest() ? PartialView("_Details", product) : View(product);
        }

        public IActionResult Create()
        {
            var product = new Product();
            return IsAjaxRequest() ? PartialView("_Create", product) : View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                return IsAjaxRequest()
                    ? PartialView("_Create", product)
                    : View(product);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = "Đã thêm sản phẩm.";

            return IsAjaxRequest()
                ? Json(new { success = true, message = "Đã thêm sản phẩm." })
                : RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return IsAjaxRequest() ? PartialView("_Edit", product) : View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return IsAjaxRequest()
                    ? PartialView("_Edit", product)
                    : View(product);
            }

            _context.Update(product);
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = "Đã cập nhật sản phẩm.";

            return IsAjaxRequest()
                ? Json(new { success = true, message = "Đã cập nhật sản phẩm." })
                : RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return IsAjaxRequest() ? PartialView("_Delete", product) : View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = "Đã xoá sản phẩm.";

            return IsAjaxRequest()
                ? Json(new { success = true, message = "Đã xoá sản phẩm." })
                : RedirectToAction(nameof(Index));
        }
    }
}
