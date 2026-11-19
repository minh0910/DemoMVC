using Microsoft.AspNetCore.Identity;
using DemoMVC.Models;

namespace DemoMVC.Data
{
    // =========================
    // Bài 13: khởi tạo role và tài khoản mặc định khi chạy ứng dụng
    // Danh sách tài khoản ở mục "DefaultUsers" trong appsettings, mật khẩu để trống thì không tạo
    // =========================
    public static class IdentityDataInitializer
    {
        public const string AdminRole = "Admin";
        public static readonly string[] DefaultRoles = { AdminRole, "QuanLy", "NhanVien" };

        public static async Task SeedAsync(IServiceProvider services, IConfiguration config, ILogger logger)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Tạo role
            foreach (var roleName in DefaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Tạo tài khoản
            foreach (var item in config.GetSection("DefaultUsers").GetChildren())
            {
                var email = item["Email"];
                var matKhau = item["Password"];
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(matKhau))
                {
                    continue;
                }

                if (await userManager.FindByEmailAsync(email) != null)
                {
                    continue; // đã có rồi
                }

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = item["FullName"],
                    EmailConfirmed = true // tài khoản mặc định không phải xác thực email
                };

                var ketQua = await userManager.CreateAsync(user, matKhau);
                if (!ketQua.Succeeded)
                {
                    logger.LogError("Tạo tài khoản {Email} thất bại: {Errors}", email,
                        string.Join("; ", ketQua.Errors.Select(e => e.Description)));
                    continue;
                }

                var role = item["Role"];
                if (!string.IsNullOrEmpty(role) && await roleManager.RoleExistsAsync(role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }

                logger.LogInformation("Đã tạo tài khoản mặc định {Email} ({Role})", email, role);
            }
        }
    }
}
