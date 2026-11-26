using DemoMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
            var rows = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                rows.Add(new UserRoleViewModel
                {
                    User = user,
                    Roles = await _userManager.GetRolesAsync(user)
                });
            }

            ViewBag.AllRoles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            return View(rows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoles(string userId, string[] roleNames)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            roleNames ??= Array.Empty<string>();
            var validRoleNames = await _roleManager.Roles
                .Where(r => roleNames.Contains(r.Name!))
                .Select(r => r.Name!).ToListAsync();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                TempData["Error"] = string.Join("; ", removeResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            var addResult = validRoleNames.Count == 0
                ? IdentityResult.Success
                : await _userManager.AddToRolesAsync(user, validRoleNames);

            TempData[addResult.Succeeded ? "Success" : "Error"] = addResult.Succeeded
                ? "Đã cập nhật role cho người dùng"
                : string.Join("; ", addResult.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }
    }

    public class UserRoleViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
