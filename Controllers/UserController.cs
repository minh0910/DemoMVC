using DemoMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string? keyword)
        {
            var usersQuery = _userManager.Users.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var search = keyword.Trim();
                usersQuery = usersQuery.Where(u =>
                    (u.Email != null && u.Email.Contains(search)) ||
                    (u.UserName != null && u.UserName.Contains(search)) ||
                    (u.FullName != null && u.FullName.Contains(search)));
            }

            var users = await usersQuery.OrderBy(u => u.Email).ToListAsync();
            var rows = new List<UserRoleViewModel>(users.Count);
            foreach (var user in users)
            {
                rows.Add(new UserRoleViewModel
                {
                    User = user,
                    Roles = await _userManager.GetRolesAsync(user)
                });
            }

            ViewBag.AllRoles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            ViewBag.Keyword = keyword;
            return View(rows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoles(string userId, string[]? roleNames, string? keyword)
        {
            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null)
                return NotFound();

            roleNames ??= Array.Empty<string>();
            var requestedNames = roleNames
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var validRoles = await _roleManager.Roles.ToListAsync();
            var selectedRoles = validRoles
                .Where(role => requestedNames.Contains(role.Name!, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (selectedRoles.Count != requestedNames.Length)
            {
                TempData["Error"] = "Danh sách Role gửi lên không hợp lệ. Vui lòng tải lại trang.";
                return RedirectToAction(nameof(Index), new { keyword });
            }

            var currentRoles = await _userManager.GetRolesAsync(targetUser);
            var requestedRoleNames = selectedRoles.Select(role => role.Name!).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var currentRoleNames = currentRoles.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var rolesToRemove = currentRoles
                .Where(role => !requestedRoleNames.Contains(role))
                .ToArray();
            var rolesToAdd = selectedRoles
                .Select(role => role.Name!)
                .Where(role => !currentRoleNames.Contains(role))
                .ToArray();

            // Tránh để admin tự khoá quyền quản trị của chính mình.
            var currentUserId = _userManager.GetUserId(User);
            var adminRole = validRoles.FirstOrDefault(r => r.Name == "Admin");
            if (targetUser.Id == currentUserId && adminRole != null && rolesToRemove.Contains(adminRole.Name!, StringComparer.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Bạn không thể tự gỡ Role Admin của chính mình.";
                return RedirectToAction(nameof(Index), new { keyword });
            }

            // Không cho gỡ Admin khỏi admin cuối cùng.
            if (adminRole != null && rolesToRemove.Contains(adminRole.Name!, StringComparer.OrdinalIgnoreCase))
            {
                var adminUsers = await _userManager.GetUsersInRoleAsync(adminRole.Name!);
                if (adminUsers.Count <= 1)
                {
                    TempData["Error"] = "Không thể gỡ Role Admin của tài khoản Admin cuối cùng.";
                    return RedirectToAction(nameof(Index), new { keyword });
                }
            }

            if (rolesToRemove.Length > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(targetUser, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    TempData["Error"] = string.Join("; ", removeResult.Errors.Select(error => error.Description));
                    return RedirectToAction(nameof(Index), new { keyword });
                }
            }

            if (rolesToAdd.Length > 0)
            {
                var addResult = await _userManager.AddToRolesAsync(targetUser, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    // Khôi phục những Role vừa gỡ nếu thao tác thêm thất bại.
                    if (rolesToRemove.Length > 0)
                        await _userManager.AddToRolesAsync(targetUser, rolesToRemove);

                    TempData["Error"] = string.Join("; ", addResult.Errors.Select(error => error.Description));
                    return RedirectToAction(nameof(Index), new { keyword });
                }
            }

            TempData["Success"] = $"Đã cập nhật Role cho {targetUser.Email ?? targetUser.UserName}.";
            return RedirectToAction(nameof(Index), new { keyword });
        }
    }

    public class UserRoleViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
