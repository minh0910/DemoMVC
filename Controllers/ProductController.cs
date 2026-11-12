using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoMVC.Data;
using DemoMVC.Models;

namespace DemoMVC.Controllers
{
    // Bài 11: CRUD sản phẩm bằng Modal + Ajax
    public class ProductController : AjaxCrudController
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET: Product (trang khung, bảng được nạp bằng Ajax)
        // =========================
        public IActionResult Index()
        {
            return View();
        }


        // =========================
        // GET: Product/List (Ajax)
        // =========================
        public async Task<IActionResult> List()
        {
            var products = await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return PartialView("_Table", products);
        }


        // =========================
        // GET: Product/Details/5 (Ajax)
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return PartialView("_Details", product);
        }


        // =========================
        // GET: Product/Form hoặc Product/Form/5 (Ajax)
        // =========================
        public async Task<IActionResult> Form(int? id)
        {
            if (id == null)
            {
                return PartialView("_Form", new Product());
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return PartialView("_Form", product);
        }


        // =========================
        // POST: Product/Save (Ajax) - Id = 0 là thêm mới, khác 0 là sửa
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([Bind("Id,Name,Price")] Product input)
        {
            if (!ModelState.IsValid)
            {
                return FormWithErrors(input);
            }

            if (input.Id == 0)
            {
                input.CreatedAt = DateTime.UtcNow;
                _context.Products.Add(input);

                await _context.SaveChangesAsync();

                return JsonOk($"Đã thêm sản phẩm {input.Name}");
            }

            // Sửa: lấy bản ghi trong CSDL để giữ nguyên ngày tạo
            var product = await _context.Products.FindAsync(input.Id);

            if (product == null)
            {
                return JsonFail("Sản phẩm không còn tồn tại");
            }

            product.Name = input.Name;
            product.Price = input.Price;

            await _context.SaveChangesAsync();

            return JsonOk($"Đã cập nhật sản phẩm {product.Name}");
        }


        // =========================
        // POST: Product/Delete/5 (Ajax)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return JsonFail("Sản phẩm không còn tồn tại");
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return JsonOk($"Đã xoá sản phẩm {product.Name}");
        }
    }
}
