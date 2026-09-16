# Lesson 4: Tìm hiểu về định tuyến (Routing) trong .NET MVC

**Routing** là cơ chế ánh xạ một URL tới **Controller** và **Action** sẽ xử lý URL đó. ASP.NET Core MVC có 2 cách:

- **Định tuyến theo quy ước** (conventional routing): khai báo chung trong `Program.cs`.
- **Định tuyến bằng thuộc tính** (attribute routing): đặt `[Route]` ngay trên controller/action.

> Ví dụ chạy thử: trang `/DinhTuyen` trong dự án, code ở `Controllers/DinhTuyenController.cs` và `Controllers/CuaHangController.cs`.

## 1. Cấu trúc URL trong MVC

Route mặc định trong `Program.cs`:

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

| Thành phần | Ý nghĩa |
|---|---|
| `{controller=Home}` | Đoạn thứ 1 là tên controller (bỏ hậu tố *Controller*). Không có thì mặc định là `Home`. |
| `{action=Index}` | Đoạn thứ 2 là tên action. Không có thì mặc định là `Index`. |
| `{id?}` | Đoạn thứ 3 là tham số **tuỳ chọn** (dấu `?`), gán vào tham số `id` của action. |
| `?tab=mo-ta` | Query string, cũng được gán vào tham số cùng tên của action. |

Phân tích URL `https://localhost:7197/DinhTuyen/ChiTiet/5?tab=mo-ta`:

| Phần | Giá trị |
|---|---|
| Giao thức + host + port | `https://localhost:7197` |
| controller | `DinhTuyen` → `DinhTuyenController` |
| action | `ChiTiet` → phương thức `ChiTiet(...)` |
| id | `5` |
| query string | `tab=mo-ta` |

```csharp
public class DinhTuyenController : Controller
{
    // /DinhTuyen/ChiTiet/5?tab=mo-ta  →  id = 5, tab = "mo-ta"
    public IActionResult ChiTiet(int? id, string? tab) { ... }
}
```

Một số URL và kết quả:

| URL | Controller | Action | id |
|---|---|---|---|
| `/` | Home | Index | null |
| `/DinhTuyen` | DinhTuyen | Index | null |
| `/DinhTuyen/ChiTiet` | DinhTuyen | ChiTiet | null |
| `/dinhtuyen/chitiet/5` | DinhTuyen | ChiTiet | 5 (không phân biệt hoa thường) |

### Tạo link đúng route

Nên dùng Tag Helper hoặc `Url.Action` thay vì viết cứng URL. Khi đổi route, link sẽ tự cập nhật.

```html
<a asp-controller="DinhTuyen" asp-action="ChiTiet" asp-route-id="5">Chi tiết</a>
<!-- sinh ra: <a href="/DinhTuyen/ChiTiet/5"> -->

@Url.Action("SanPham", "DinhTuyen", new { id = 7 })
<!-- sinh ra: /san-pham/7 (vì action SanPham dùng [Route]) -->
```

## 2. Lỗi khi truy cập URL trong MVC

| Tình huống | Mã lỗi | Nguyên nhân / cách xử lý |
|---|---|---|
| Sai tên controller (`/KhongCo/Index`) | **404** | Không có class `KhongCoController`. Kiểm tra chính tả; class phải `public` và kế thừa `Controller`. |
| Sai tên action (`/DinhTuyen/KhongCoAction`) | **404** | Controller có nhưng không có phương thức public tên đó. |
| Vi phạm ràng buộc route (`/san-pham/abc`) | **404** | Route `san-pham/{id:int}` yêu cầu số nguyên, `abc` không khớp. |
| Gọi action có `[Route]` bằng URL mặc định (`/DinhTuyen/SanPham/5`) | **404** | Action đã gắn attribute route thì **không** truy cập được qua route mặc định nữa, phải dùng `/san-pham/5`. |
| Sai phương thức HTTP | **405** | Action chỉ nhận `[HttpPost]` nhưng bị gọi bằng phương thức khác. |
| Tham số sai kiểu (`/DinhTuyen/ChiTiet/abc`) | **200** | Route mặc định không ràng buộc kiểu nên vẫn khớp, nhưng model binding thất bại: `id = null`, `ModelState.IsValid = false`. Cần kiểm tra `ModelState`. |
| Không tìm thấy View | **500** | `InvalidOperationException: The view 'ThieuView' was not found`. Tạo file `Views/DinhTuyen/ThieuView.cshtml` hoặc chỉ định đúng tên view. |
| Hai action trùng route | **500** | `AmbiguousMatchException`: nhiều endpoint cùng khớp. Đổi route hoặc phân biệt bằng `[HttpGet]` / `[HttpPost]`. |

> **Lưu ý về 405 khi chạy local:** ở môi trường Development, `app.MapStaticAssets()` thêm một endpoint dự phòng `{**path:file}` nhận mọi request GET. Vì vậy gõ URL chỉ nhận POST trên trình duyệt sẽ ra **404** thay vì 405. Gửi bằng PUT/DELETE, hoặc chạy ở Production, sẽ thấy 405.

### Hiển thị trang lỗi thân thiện

Mặc định lỗi 404/405 chỉ trả về trang trắng. Thêm middleware vào `Program.cs` để chuyển tới một action hiển thị lỗi:

```csharp
// Lỗi theo mã trạng thái (404, 405...) → chạy lại request tới /Home/Loi?code=404
app.UseStatusCodePagesWithReExecute("/Home/Loi", "?code={0}");

// Lỗi 500 (exception) ở môi trường Production
app.UseExceptionHandler("/Home/Error");
```

```csharp
public class HomeController : Controller
{
    public IActionResult Loi(int code)
    {
        ViewBag.Code = code;
        return View();
    }
}
```

## 3. Cơ chế định tuyến với `[Route]`

Attribute routing đặt route ngay trên controller/action. URL thân thiện hơn (tốt cho SEO) và linh hoạt hơn route mặc định.

```csharp
public class DinhTuyenController : Controller
{
    [Route("san-pham/{id:int}")]                   // /san-pham/5
    public IActionResult SanPham(int id) { ... }

    [Route("tin-tuc/{nam:int:min(2000)}/{slug}")]  // /tin-tuc/2026/hoc-mvc
    public IActionResult TinTuc(int nam, string slug) { ... }

    [Route("gioi-thieu")]                          // nhiều route cho 1 action
    [Route("about")]
    public IActionResult GioiThieu() { ... }

    [Route("trang/{so:int=1}")]                    // /trang và /trang/3 (mặc định so = 1)
    public IActionResult Trang(int so) { ... }

    [HttpPost("dinh-tuyen/chi-post")]              // route + chỉ nhận POST
    public IActionResult ChiPost() { ... }
}

[Route("cua-hang")]                                // tiền tố chung cho cả controller
public class CuaHangController : Controller
{
    [HttpGet("")]                                  // /cua-hang
    public IActionResult Index() { ... }

    [HttpGet("danh-muc/{ten}")]                    // /cua-hang/danh-muc/dien-thoai
    public IActionResult DanhMuc(string ten) { ... }

    [HttpGet("[action]")]                          // token [action] → /cua-hang/LienHe
    public IActionResult LienHe() { ... }
}
```

### Các điểm cần nhớ

- **Ràng buộc (constraint)**: `int`, `bool`, `guid`, `datetime`, `min(x)`, `max(x)`, `range(a,b)`, `length(n)`, `alpha`, `regex(...)`. Giá trị không thoả thì route không khớp → **404**.
- **Tham số tuỳ chọn** `{id?}` và **giá trị mặc định** `{so=1}`.
- **Token** `[controller]`, `[action]` được thay bằng tên thật, ví dụ `[Route("[controller]/[action]")]`.
- Route ở action bắt đầu bằng `/` hoặc `~/` sẽ **bỏ qua** tiền tố của controller.
- `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]` vừa khai báo route vừa giới hạn phương thức HTTP.
- Action đã có attribute route thì **không dùng được route mặc định** nữa.

### So sánh 2 cách định tuyến

| | Conventional routing | Attribute routing (`[Route]`) |
|---|---|---|
| Khai báo ở | `Program.cs` | Trên controller/action |
| URL | Theo khuôn chung `/Controller/Action/id` | Tuỳ ý: `/san-pham/5`, `/tin-tuc/2026/slug` |
| Phù hợp | Ứng dụng có cấu trúc đồng nhất | URL thân thiện SEO, Web API |
