using DemoMVC.Data;
using DemoMVC.Models;
using DemoMVC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(
builder.Configuration.GetConnectionString("DefaultConnection")));

// Service sinh mã sinh viên tự động (dùng chung DbContext của request nên đăng ký Scoped)
builder.Services.AddScoped<StudentCodeGenerator>();

// =========================
// Bài 13: Identity (đăng ký, đăng nhập, xác thực email, quên mật khẩu)
// =========================
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
options.SignIn.RequireConfirmedAccount = true;   // phải xác thực email mới đăng nhập được
options.User.RequireUniqueEmail = true;
options.Password.RequiredLength = 6;
options.Password.RequireNonAlphanumeric = false;
options.Password.RequireUppercase = false;
options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

// Gửi email qua SMTP (mục "Smtp" trong appsettings)
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// Tự tạo/cập nhật bảng theo Migrations khi khởi động (hosting không chạy được "dotnet ef database update")
// Lỗi kết nối CSDL chỉ ghi log để các trang không dùng CSDL vẫn chạy bình thường
using (var scope = app.Services.CreateScope())
{
try
{
scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();

// Bài 13: tạo role + tài khoản mặc định
await IdentityDataInitializer.SeedAsync(scope.ServiceProvider, app.Configuration, app.Logger);
}
catch (Exception ex)
{
app.Logger.LogError(ex, "Không chạy được migration, kiểm tra lại connection string");
}
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
app.UseExceptionHandler("/Home/Error");
app.UseHsts();
}

// Khi gặp mã lỗi (404, 405...) sẽ chạy lại request tới /Home/Loi để hiển thị trang lỗi thân thiện
app.UseStatusCodePagesWithReExecute("/Home/Loi", "?code={0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}")
.WithStaticAssets();

// Bài 13: các trang Razor của Identity (/Identity/Account/Login...)
app.MapRazorPages()
.WithStaticAssets();

app.Run();
