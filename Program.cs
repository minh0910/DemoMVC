using DemoMVC.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(
builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Tự tạo/cập nhật bảng theo Migrations khi khởi động (hosting không chạy được "dotnet ef database update")
// Lỗi kết nối CSDL chỉ ghi log để các trang không dùng CSDL vẫn chạy bình thường
using (var scope = app.Services.CreateScope())
{
try
{
scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}")
.WithStaticAssets();

app.Run();
