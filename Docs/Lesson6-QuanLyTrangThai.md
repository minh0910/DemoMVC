# Lesson 6: Tìm hiểu về quản lý trạng thái

> Ví dụ chạy thử: trang `/TrangThai` trong dự án, code ở `Controllers/TrangThaiController.cs` và `Views/TrangThai/`.

HTTP là giao thức **phi trạng thái (stateless)**: mỗi request độc lập, server không tự nhớ request trước đó. ASP.NET Core MVC cung cấp `ViewData`, `ViewBag` và `TempData` để truyền dữ liệu từ Controller sang View, hoặc giữa các request liên tiếp.

## 1. ViewData

- Kiểu `ViewDataDictionary`: từ điển **key (`string`) → value (`object`)**.
- Truy cập bằng key: `ViewData["Lop"] = "K66-CNTT";`
- Giá trị có kiểu `object` nên **phải ép kiểu** khi dùng như kiểu cụ thể, và cần chú ý null.
- Chỉ tồn tại trong **request hiện tại**; redirect là mất.
- Hay dùng cho `ViewData["Title"]`: đặt tiêu đề trong View, Layout đọc ra để hiện trên thẻ `<title>`.

```csharp
// Controller
ViewData["DiemTB"] = 8.25;
```

```cshtml
@{ var diem = (double)ViewData["DiemTB"]!; }   @* phải ép kiểu *@
<p>Xếp loại: @(diem >= 8 ? "Giỏi" : "Khá")</p>
```

## 2. ViewBag

- Kiểu `dynamic`, là lớp bọc (wrapper) quanh `ViewData`. Truy cập bằng thuộc tính: `ViewBag.HoTen = "An";`
- **Không cần ép kiểu**, cú pháp gọn. Đổi lại không có IntelliSense, gõ sai tên thì không báo lỗi mà chỉ trả về `null` khi chạy.
- **Dùng chung dữ liệu** với ViewData: `ViewBag.X` và `ViewData["X"]` là một.
- Cũng chỉ tồn tại trong request hiện tại.

```csharp
// Controller
ViewBag.MonHoc = new[] { "C#", "ASP.NET Core", "SQL" };

ViewBag.ChungKho = "Gán bằng ViewBag, đọc bằng ViewData";
ViewData["ChungKho2"] = "Gán bằng ViewData, đọc bằng ViewBag";
```

```cshtml
@foreach (var mon in ViewBag.MonHoc) { <li>@mon</li> }

<p>@ViewData["ChungKho"]</p>          @* → Gán bằng ViewBag, đọc bằng ViewData *@
<p>@ViewBag.ChungKho2</p>             @* → Gán bằng ViewData, đọc bằng ViewBag *@
<p>@(ViewBag.SaiTen ?? "null")</p>    @* gõ sai tên: không lỗi, chỉ ra null *@
```

## 3. TempData

- Kiểu `ITempDataDictionary`, truy cập bằng key giống ViewData.
- Dữ liệu **vẫn còn sau khi redirect**, tồn tại đến khi được **đọc** ở một request sau thì bị xoá.
- Mặc định lưu trong **cookie** (đã mã hoá); có thể cấu hình để lưu bằng Session.
- Chỉ lưu được kiểu đơn giản (`string`, `int`, `bool`, `DateTime`…). Đối tượng phức tạp cần chuyển sang JSON.
- Dùng điển hình: hiện thông báo "Thêm thành công" sau khi POST rồi redirect (mẫu Post/Redirect/Get).

```csharp
[HttpPost]
public IActionResult ThemMoi(SinhVien sv)
{
    _ds.Add(sv);
    TempData["ThongBao"] = "Thêm sinh viên thành công!";
    return RedirectToAction("DanhSach");   // ViewBag sẽ mất, TempData vẫn còn
}
```

```cshtml
@* DanhSach.cshtml *@
@if (TempData["ThongBao"] != null)
{
    <div class="alert alert-success">@TempData["ThongBao"]</div>
}
```

### Peek và Keep

| Cách đọc | Kết quả |
|---|---|
| `TempData["key"]` | Đọc và **đánh dấu xoá**; hết request thì bị xoá |
| `TempData.Peek("key")` | Đọc nhưng **không** đánh dấu xoá |
| `TempData.Keep("key")` | Huỷ đánh dấu xoá, giữ giá trị cho request tiếp theo |

```csharp
public IActionResult GuiTempData(string cheDo = "doc")
{
    TempData["ThongBao"] = $"TempData được gán lúc {DateTime.Now:HH:mm:ss}";
    ViewBag.ThongBao = "ViewBag được gán trước khi redirect";
    return RedirectToAction(nameof(NhanTempData), new { cheDo });
}

public IActionResult NhanTempData(string cheDo = "doc")
{
    ViewBag.TempDataDocDuoc = cheDo == "peek"
        ? TempData.Peek("ThongBao")    // đọc, không xoá
        : TempData["ThongBao"];        // đọc, đánh dấu xoá

    if (cheDo == "keep")
        TempData.Keep("ThongBao");     // huỷ đánh dấu xoá

    return View();
}
```

Kết quả khi chạy thử:

| Chế độ | Sau redirect | Tải lại trang lần 1 | Tải lại trang lần 2 |
|---|---|---|---|
| Đọc thường | Có giá trị | `null` | `null` |
| Peek | Có giá trị | Có giá trị | Có giá trị |
| Keep | Có giá trị | Có giá trị | Có giá trị |
| *(ViewBag.ThongBao)* | `null` | `null` | `null` |

`ViewBag` luôn `null` ở trang nhận, vì redirect tạo ra một request mới.

## 4. So sánh ViewData, ViewBag, TempData

| Tiêu chí | ViewData | ViewBag | TempData |
|---|---|---|---|
| Kiểu | `ViewDataDictionary` | `dynamic` | `ITempDataDictionary` |
| Cú pháp | `ViewData["Key"]` | `ViewBag.Key` | `TempData["Key"]` |
| Ép kiểu khi đọc | Cần | Không cần | Cần |
| Kiểm tra khi biên dịch | Không | Không | Không |
| Thời gian sống | 1 request | 1 request | Đến khi được đọc (qua được redirect) |
| Nơi lưu | Bộ nhớ server | Bộ nhớ server (chung với ViewData) | Cookie (mặc định) hoặc Session |
| Hướng truyền | Controller → View, View → Layout/Partial | Controller → View | Controller → Controller → View |
| Khi nào dùng | Dữ liệu nhỏ, tiêu đề trang | Dữ liệu nhỏ, viết nhanh | Thông báo sau redirect |

## 5. Kết luận

- Dữ liệu **chính** của trang nên truyền bằng **Model mạnh kiểu** (`return View(model)` + `@model`), xem Lesson 5.
- `ViewData` / `ViewBag` hợp với dữ liệu phụ, nhỏ trong cùng một request.
- `TempData` hợp với dữ liệu cần giữ qua **một lần redirect**, điển hình là thông báo kết quả.
