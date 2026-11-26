using System.Reflection;
using DemoMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Data
{
    public static class IdentityDataInitializer
    {
        public const string AdminRole = "Admin";
        public static readonly string[] DefaultRoles = { AdminRole, "QuanLy", "NhanVien" };

        public static async Task SeedAsync(IServiceProvider services, IConfiguration config, ILogger logger)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var context = services.GetRequiredService<ApplicationDbContext>();

            foreach (var roleName in DefaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (!result.Succeeded)
                        logger.LogError("Tạo role {Role} thất bại: {Errors}", roleName,
                            string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }

            foreach (var item in config.GetSection("DefaultUsers").GetChildren())
            {
                var email = item["Email"];
                var password = item["Password"];
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    continue;

                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = item["FullName"],
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(user, password);
                    if (!createResult.Succeeded)
                    {
                        logger.LogError("Tạo tài khoản {Email} thất bại: {Errors}", email,
                            string.Join("; ", createResult.Errors.Select(e => e.Description)));
                        continue;
                    }
                }

                var role = item["Role"];
                if (!string.IsNullOrWhiteSpace(role) && await roleManager.RoleExistsAsync(role)
                    && !await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }

            await SyncActionPermissionsAsync(services, context, roleManager, logger);
        }

        private static async Task SyncActionPermissionsAsync(IServiceProvider services,
            ApplicationDbContext context, RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            var descriptors = services.GetRequiredService<IActionDescriptorCollectionProvider>()
                .ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .Where(IsProtectedAction)
                .GroupBy(a => (a.ControllerName, a.ActionName))
                .Select(group => group.First())
                .ToList();

            foreach (var descriptor in descriptors)
            {
                var permission = await context.Permissions.SingleOrDefaultAsync(p =>
                    p.Controller == descriptor.ControllerName && p.Action == descriptor.ActionName);

                var displayName = $"{descriptor.ControllerName} - {descriptor.ActionName}";
                if (permission == null)
                {
                    context.Permissions.Add(new Permission
                    {
                        Controller = descriptor.ControllerName,
                        Action = descriptor.ActionName,
                        DisplayName = displayName
                    });
                }
                else if (permission.DisplayName != displayName)
                {
                    permission.DisplayName = displayName;
                }
            }

            await context.SaveChangesAsync();

            // Seed mặc định chỉ cho role vừa tạo quyền lần đầu; không ghi đè các lựa chọn quản trị đã lưu.
            var permissions = await context.Permissions.AsNoTracking().ToListAsync();
            foreach (var roleName in new[] { "QuanLy", "NhanVien" })
            {
                var role = await roleManager.FindByNameAsync(roleName);
                if (role == null)
                    continue;

                var hasAssignments = await context.RolePermissions.AnyAsync(rp => rp.RoleId == role.Id);
                if (hasAssignments)
                    continue;

                var defaultPermissions = roleName == "QuanLy"
                    ? permissions.Where(p => p.Controller != "Role" && p.Controller != "User")
                    : permissions.Where(p => p.Controller != "Role" && p.Controller != "User"
                        && (p.Action == "Index" || p.Action == "List" || p.Action == "Details" || p.Action == "NextCode"));

                context.RolePermissions.AddRange(defaultPermissions.Select(permission => new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                }));
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Đã đồng bộ {Count} quyền Controller/Action", descriptors.Count);
        }

        private static bool IsProtectedAction(ControllerActionDescriptor descriptor)
        {
            return descriptor.MethodInfo.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() == null
                && descriptor.ControllerTypeInfo.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() == null;
        }
    }
}
