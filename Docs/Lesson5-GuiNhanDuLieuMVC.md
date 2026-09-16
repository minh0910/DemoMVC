# Lesson 5: Tìm hiểu về gửi nhận dữ liệu giữa M-V-C

> Ví dụ chạy thử: trang `/DuLieu` trong dự án, code ở `Controllers/DuLieuController.cs`, `Models/SinhVien.cs` và `Views/DuLieu/`.

## 1. Controller, View, Model

### Controller
- Class trong `Controllers/`, kế thừa `Controller`, tên kết thúc bằng `Controller`.
- Mỗi phương thức `public` là một **action**, trả về `IActionResult`:
  `View()`, `RedirectToAction()`, `Json()`, `Content()`, `NotFound()`…
- Dùng `[HttpGet]` / `[HttpPost]` để tách action **hiển thị form** và action **xử lý form**.

### View
- File `.cshtml` nằm trong `Views/<TênController>/`.
- Cú pháp **Razor**: `@bien`, `@{ code C# }`, `@if`, `@foreach`.
- `@model KieuDuLieu` khai báo View mạnh kiểu (strongly-typed).
- **Tag Helper**: `asp-for`, `asp-action`, `asp-route-id`, `asp-validation-for`…

### Model
- Class C# trong `Models/` mô tả dữ liệu.
- **Data Annotations** để kiểm tra dữ liệu: `[Required]`, `[Range]`, `[StringLength]`, `[EmailAddress]`, `[Display]`.
- **Model binding**: ASP.NET Core tự gán dữ liệu từ request vào tham số/Model.

```csharp
public class SinhVien
{
    [Display(Name = "Mã sinh viên")]
    [Required(ErrorMessage = "Vui lòng nhập mã sinh viên")]
    [StringLength(10, ErrorMessage = "Mã sinh viên tối đa 10 ký tự")]
    public string MaSV { get; set; } = "";

    [Display(Name = "Họ tên")]
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 50 ký tự")]
    public string HoTen { get; set; } = "";

    [Display(Name = "Tuổi")]
    [Range(16, 60, ErrorMessage = "Tuổi phải từ 16 đến 60")]
    public int Tuoi { get; set; }

    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string? Email { get; set; }

    [Display(Name = "Khoa")]
    public string Khoa { get; set; } = "CNTT";
}
```

## 2. Các hướng truyền dữ liệu

| Hướng | Cách thực hiện |
|---|---|
| **Controller → View** | `ViewBag`, `ViewData`, `TempData` (xem Lesson 6), hoặc `return View(model)` |
| **View → Controller** | Form (GET/POST), query string, route value. Controller nhận qua **tham số action**, `IFormCollection`, `Request.Form`, `Request.Query` |
| **Qua Model** | Controller gửi đối tượng bằng `View(model)`; View dùng `@model` + `asp-for`; khi submit, model binding tạo lại đối tượng và kiểm tra `ModelState.IsValid` |

## 3. Ví dụ gửi nhận dữ liệu giữa Controller và View

### 3.1. Controller → View

```csharp
public IActionResult ControllerSangView()
{
    ViewBag.TieuDe = "Xin chào từ Controller!";
    ViewData["ThoiGian"] = DateTime.Now;
    ViewBag.DsMonHoc = new List<string> { "Lập trình C#", "ASP.NET Core MVC", "Cơ sở dữ liệu" };
    return View();
}
```

```cshtml
<h4>@ViewBag.TieuDe</h4>
<p>Thời gian: @(((DateTime)ViewData["ThoiGian"]!).ToString("HH:mm:ss dd/MM/yyyy"))</p>
<ul>
    @foreach (var mon in ViewBag.DsMonHoc)
    {
        <li>@mon</li>
    }
</ul>
```

- `ViewData` có kiểu `object` nên phải **ép kiểu** trước khi gọi phương thức.
- `ViewBag` có kiểu `dynamic` nên không cần ép kiểu, nhưng không được kiểm tra khi biên dịch.

### 3.2. View → Controller qua form GET (query string)

Thuộc tính `name` của input phải **trùng tên tham số** của action (không phân biệt hoa thường).

```cshtml
<form asp-action="ViewSangController" method="get">
    <input name="tuKhoa" />
    <button type="submit">Tìm</button>
</form>
<!-- URL gửi đi: /DuLieu/ViewSangController?tuKhoa=an -->
```

```csharp
[HttpGet]
public IActionResult ViewSangController(string? tuKhoa)
{
    if (tuKhoa != null)
    {
        ViewBag.KetQuaTim = _dsSinhVien
            .Where(s => s.HoTen.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase))
            .Select(s => s.HoTen)
            .ToList();
    }
    return View();
}
```

### 3.3. View → Controller qua form POST

```cshtml
<form asp-action="ViewSangController" method="post">
    <input name="hoTen" />
    <input name="namSinh" type="number" />
    <select name="gioiTinh">
        <option>Nam</option>
        <option>Nữ</option>
    </select>
    <button type="submit">Gửi</button>
</form>
```

```csharp
[HttpPost]
public IActionResult ViewSangController(string hoTen, int namSinh, IFormCollection form)
{
    ViewBag.HoTen = hoTen;                        // nhận qua tham số
    ViewBag.Tuoi = DateTime.Now.Year - namSinh;
    ViewBag.GioiTinh = form["gioiTinh"];          // nhận qua IFormCollection
    return View();
}
```

## 4. Ví dụ gửi nhận dữ liệu thông qua Model

### 4.1. Truyền 1 đối tượng sang View

```csharp
// /DuLieu/ThongTin/SV001
public IActionResult ThongTin(string id = "SV001")
{
    var sv = _dsSinhVien.FirstOrDefault(s => s.MaSV == id);
    if (sv == null) return NotFound();
    return View(sv);                               // truyền Model
}
```

```cshtml
@model SinhVien

<dl>
    <dt>@Html.DisplayNameFor(m => m.HoTen)</dt>   <!-- "Họ tên" lấy từ [Display] -->
    <dd>@Model.HoTen</dd>
    <dt>@Html.DisplayNameFor(m => m.Tuoi)</dt>
    <dd>@Model.Tuoi</dd>
</dl>
```

Ưu điểm so với ViewBag: có **IntelliSense**, được **kiểm tra kiểu khi biên dịch**, gõ sai tên thuộc tính sẽ báo lỗi ngay.

### 4.2. Truyền danh sách sang View

```csharp
public IActionResult DanhSach()
{
    return View(_dsSinhVien);                      // List<SinhVien>
}
```

```cshtml
@model List<SinhVien>

<table>
    @foreach (var sv in Model)
    {
        <tr>
            <td>@sv.MaSV</td>
            <td>@sv.HoTen</td>
            <td><a asp-action="ThongTin" asp-route-id="@sv.MaSV">Chi tiết</a></td>
        </tr>
    }
</table>
```

### 4.3. View gửi Model về Controller (model binding + validation)

```cshtml
@model SinhVien

<form asp-action="ThemMoi" method="post">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>

    <label asp-for="HoTen"></label>
    <input asp-for="HoTen" class="form-control" />
    <span asp-validation-for="HoTen" class="text-danger"></span>

    <label asp-for="Tuoi"></label>
    <input asp-for="Tuoi" class="form-control" />
    <span asp-validation-for="Tuoi" class="text-danger"></span>

    <button type="submit">Lưu</button>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

```csharp
[HttpGet]
public IActionResult ThemMoi()
{
    return View(new SinhVien());
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult ThemMoi(SinhVien sv)          // model binding tạo đối tượng từ form
{
    if (_dsSinhVien.Any(s => s.MaSV == sv.MaSV))
        ModelState.AddModelError(nameof(SinhVien.MaSV), "Mã sinh viên đã tồn tại");

    if (!ModelState.IsValid)
        return View(sv);                           // trả lại form + dữ liệu đã nhập + lỗi

    _dsSinhVien.Add(sv);
    return RedirectToAction(nameof(DanhSach));
}
```

Giải thích:
- `asp-for="HoTen"` sinh ra `name="HoTen"`, `id`, `value`, kiểu input và các thuộc tính kiểm tra dữ liệu lấy từ Data Annotations.
- **Model binding** tạo đối tượng `SinhVien` từ các field trong form có tên trùng với thuộc tính.
- Dữ liệu được kiểm tra 2 lớp:
  - **Client**: jQuery Validation kiểm tra trước khi gửi (nhờ `_ValidationScriptsPartial`).
  - **Server**: `ModelState.IsValid`, **luôn bắt buộc** vì kiểm tra ở client có thể bị bỏ qua.
- `ModelState.AddModelError` để thêm lỗi nghiệp vụ tự viết (ví dụ trùng mã).
- Form tag helper tự thêm token chống giả mạo (CSRF), kiểm tra bằng `[ValidateAntiForgeryToken]`.
- Mẫu **Post/Redirect/Get**: lưu xong thì redirect, để nhấn F5 không gửi lại form.
