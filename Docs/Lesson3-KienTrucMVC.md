# Lesson 3: Tìm hiểu về kiến trúc .NET MVC

## 1. MVC là gì?

**MVC (Model – View – Controller)** là mẫu kiến trúc chia ứng dụng thành 3 phần, mỗi phần một nhiệm vụ riêng (*Separation of Concerns*). Nhờ vậy code dễ đọc, dễ test và dễ bảo trì.

| Thành phần | Nhiệm vụ | Trong dự án |
|---|---|---|
| **Model** | Dữ liệu và nghiệp vụ: class C#, quy tắc kiểm tra dữ liệu, truy cập database | Thư mục `Models/`, ví dụ `SinhVien.cs` |
| **View** | Giao diện hiển thị dữ liệu cho người dùng | File `.cshtml` (HTML + cú pháp Razor `@`) trong `Views/` |
| **Controller** | Điều phối: nhận request, gọi Model xử lý, chọn View và truyền dữ liệu cho View | Class trong `Controllers/`, ví dụ `HomeController.cs` |

## 2. Luồng xử lý một request

```
Trình duyệt gửi GET /SanPham/ChiTiet/5
  → Kestrel (web server)
  → Middleware pipeline
  → Routing chọn Controller + Action
  → Model binding (id = 5)
  → Action gọi Model xử lý
  → Trả về View + dữ liệu
  → Razor render ra HTML
  → Response gửi về trình duyệt
```

1. **Kestrel** là web server có sẵn trong ASP.NET Core, nhận HTTP request. Khi chạy thật có thể đặt sau IIS hoặc Nginx.
2. Request đi qua chuỗi **middleware**: chuyển sang HTTPS, file tĩnh, routing, phân quyền…
3. **Routing** so khớp URL với các route để tìm Controller và Action.
4. **Model binding** lấy dữ liệu từ route, query string, form… rồi gán vào tham số của action.
5. **Action** xử lý và trả về một `IActionResult`: `View()`, `RedirectToAction()`, `Json()`, `NotFound()`…
6. **Razor view engine** ghép View + Layout + dữ liệu thành HTML gửi về trình duyệt.

## 3. Cấu trúc thư mục dự án

```
DemoMVC/
├── Controllers/                 # Các Controller
├── Models/                      # Các class dữ liệu
├── Views/
│   ├── Home/                    # View của HomeController (tên thư mục = tên controller)
│   │   └── Index.cshtml         # tên file = tên action
│   ├── Shared/                  # View dùng chung: _Layout.cshtml, Error.cshtml
│   ├── _ViewImports.cshtml      # using và TagHelper dùng chung cho mọi view
│   └── _ViewStart.cshtml        # chạy trước mỗi view, khai báo Layout mặc định
├── wwwroot/                     # File tĩnh: css, js, lib (bootstrap, jquery), ảnh
├── Properties/launchSettings.json  # cấu hình khi chạy local (port, môi trường)
├── appsettings.json             # cấu hình ứng dụng (chuỗi kết nối DB...)
├── Program.cs                   # điểm khởi động: đăng ký service + cấu hình pipeline
└── DemoMVC.csproj               # file project: target framework, package
```

## 4. Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// (1) Đăng ký service vào Dependency Injection container
builder.Services.AddControllersWithViews();

var app = builder.Build();

// (2) Cấu hình middleware pipeline – thứ tự rất quan trọng
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

// (3) Khai báo route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

- `builder.Services...`: đăng ký các dịch vụ ứng dụng cần dùng. `AddControllersWithViews()` bật MVC gồm Controller và View.
- `app.Use...`: thêm middleware. Request đi qua các middleware **theo thứ tự khai báo**.
- `app.MapControllerRoute(...)`: khai báo cách ánh xạ URL tới Controller/Action (xem Lesson 4).

## 5. Quy ước đặt tên (Convention over Configuration)

- Controller là class kế thừa `Controller`, tên kết thúc bằng `Controller`: `HomeController` → trên URL dùng `Home`.
- Mỗi phương thức `public` trong controller là một **action**.
- `return View()` trong action `HomeController.Index` sẽ tìm `Views/Home/Index.cshtml`. Không thấy thì tìm tiếp `Views/Shared/Index.cshtml`.
- View mặc định dùng Layout `Views/Shared/_Layout.cshtml` (khai báo trong `_ViewStart.cshtml`).

## 6. Ưu điểm và hạn chế

**Ưu điểm**
- Tách biệt giao diện, xử lý và dữ liệu.
- Dễ viết unit test cho Controller và Model.
- Nhiều người làm song song được (front-end làm View, back-end làm Controller/Model).
- Kiểm soát hoàn toàn HTML và URL, thân thiện với SEO.

**Hạn chế**
- Nhiều file, nhiều thư mục, hơi cồng kềnh với dự án nhỏ.
- Phải nắm quy ước đặt tên thì framework mới tìm đúng Controller/View.
- Controller dễ bị "phình to" nếu dồn hết logic vào đó.
