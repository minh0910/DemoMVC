using DemoMVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Controllers
{
    [Authorize]
    public class RoleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<IActionViewResult?> Index()
        {
            var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            name = name?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Tên role không được để trống";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(name));
            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? $"Đã tạo role {name}"
                : string.Join("; ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rename(string id, string name)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            name = name?.Trim() ?? string.Empty;
            if (role.Name == "Admin")
            {
                TempData["Error"] = "Không thể đổi tên role Admin";
                return RedirectToAction(nameof(Index));
            }

            role.Name = name;
            var result = await _roleManager.UpdateAsync(role);
            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? "Đã cập nhật role"
                : string.Join("; ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();
            if (role.Name == "Admin")
            {
                TempData["Error"] = "Không thể xoá role Admin";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.DeleteAsync(role);
            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? "Đã xoá role"
                : string.Join("; ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Permissions(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            var permissions = await _context.Permissions
                .Include(p => p.RolePermissions)
                .OrderBy(p => p.Controller).ThenBy(p => p.Action).ToListAsync();

            ViewBag.Role = role;
            ViewBag.SelectedIds = permissions
                .Where(p => p.RolePermissions.Any(rp => rp.RoleId == role.Id))
                .Select(p => p.Id).ToHashSet();
            return View(permissions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePermissions(string id, int[] permissionIds)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            var existing = await _context.RolePermissions.Where(rp => rp.RoleId == role.Id).ToListAsync();
            _context.RolePermissions.RemoveRange(existing);

            var validIds = await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id))
                .Select(p => p.Id).ToListAsync();

            _context.RolePermissions.AddRange(validIds.Select(permissionId => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId
            }));

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật phân quyền";
            return RedirectToAction(nameof(Permissions), new { id });
        }
    }
}
