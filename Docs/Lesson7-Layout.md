# Lesson 7: Tìm hiểu về Layout

> Ví dụ chạy thử: trang `/BoCuc` trong dự án, code ở `Controllers/BoCucController.cs`, `Views/BoCuc/`, `Views/Shared/_Layout.cshtml`, `Views/Shared/_LayoutDemo.cshtml` và `wwwroot/css/`.

**Layout** là khung giao diện dùng chung cho nhiều trang: phần `<head>`, thanh menu, footer, các file CSS/JS. Mỗi View chỉ viết phần nội dung riêng, Layout "bọc" nội dung đó lại, nên không phải lặp lại cùng một đoạn HTML ở mọi trang.

## 1. Các file liên quan

| File | Vai trò |
| --- | --- |
| `Views/Shared/_Layout.cshtml` | Layout mặc định: head, navbar, footer, nạp Bootstrap và `site.css` |
| `Views/_ViewStart.cshtml` | Chạy trước mọi View, gán `Layout = "_Layout"` cho tất cả các trang |
| `Views/_ViewImports.cshtml` | Khai báo `@using` và Tag Helper dùng chung cho mọi View |
| `wwwroot/css/site.css` | File CSS chung của website, được Layout nạp vào |
| `Views/Shared/_Layout.cshtml.css` | CSS isolation: CSS chỉ áp dụng cho riêng `_Layout.cshtml` |

## 2. RenderBody

`@RenderBody()` là vị trí nội dung của View được chèn vào Layout. Mỗi Layout bắt buộc có đúng một `RenderBody()`.

```cshtml
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - DemoMVC</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
    @await RenderSectionAsync("Styles", required: false)
</head>
<body>
    <header>...navbar...</header>
    <div class="container">
        <main role="main">@RenderBody()</main>
    </div>
    <footer>...</footer>
    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

## 3. _ViewStart và cách chọn Layout

- `_ViewStart.cshtml` gán Layout mặc định cho mọi View: `@{ Layout = "_Layout"; }`
- View muốn dùng layout khác thì tự gán lại `Layout = "_LayoutDemo";` (ví dụ `/BoCuc/LayoutRieng`).
- `Layout = null`: trang không dùng layout nào.
- Razor tìm file layout theo tên trong thư mục của Controller, sau đó đến `Views/Shared/`.

```cshtml
@* Views/BoCuc/LayoutRieng.cshtml *@
@{
    Layout = "_LayoutDemo";
    ViewData["Title"] = "Layout riêng";
}
```

## 4. Section

- Layout khai báo chỗ trống: `@await RenderSectionAsync("Scripts", required: false)`.
- View điền vào bằng `@section Scripts { ... }`.
- `required: false`: View không có section cũng không lỗi. Nếu `true` mà View thiếu thì báo lỗi khi chạy.
- Dự án có 2 section:
  - `Styles`: nằm trong `<head>`, để View nạp thêm file CSS riêng (buổi này thêm mới).
  - `Scripts`: nằm cuối `<body>`, sau jQuery và Bootstrap, để View nạp JS riêng (ví dụ `_ValidationScriptsPartial`).

```cshtml
@section Styles {
    <link rel="stylesheet" href="~/css/bocuc.css" asp-append-version="true" />
}

@section Scripts {
    <script>
        // chạy sau khi bootstrap.bundle.min.js đã được nạp
    </script>
}
```

Layout nhận dữ liệu từ View qua `ViewData`: View gán `ViewData["Title"] = "7. Layout";`, Layout đọc `@ViewData["Title"]` để hiện trên thẻ `<title>`.

## 5. Tích hợp Bootstrap 5

Dự án dùng **Bootstrap v5.3.3**, file nằm trong `wwwroot/lib/bootstrap/` (template `dotnet new mvc` đã có sẵn).

**Cách 1: file trong `wwwroot/lib`** (dự án đang dùng): tải Bootstrap về bằng LibMan, npm hoặc tải tay. Không phụ thuộc mạng ngoài.

```html
<link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
<script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
```

**Cách 2: CDN**: không cần lưu file trong dự án, nhưng cần mạng ngoài.

```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
```

Lưu ý:

- CSS đặt trong `<head>` để trang hiện ra đã có giao diện; JS đặt **cuối** `<body>` để không chặn hiển thị.
- `bootstrap.bundle.min.js` đã gồm Popper, cần cho dropdown, tooltip, popover.
- Thẻ `<meta name="viewport" ...>` bắt buộc để responsive chạy đúng trên điện thoại.
- Buổi này đổi navbar sang `navbar-expand-xl` vì menu đã nhiều mục, màn hình nhỏ hơn 1200px sẽ thu gọn thành nút ☰.

Các thành phần dùng trong ví dụ `/BoCuc/ViDuBootstrap`: grid (`row`, `col-12 col-md-6 col-lg-3`), button, badge, card, alert đóng được, modal, bảng `table-responsive`.

## 6. Tách file CSS và tích hợp vào Layout

- Không viết `<style>` hay `style="..."` rải rác trong từng View: khó sửa, trùng lặp, trình duyệt không cache được.
- CSS dùng chung viết vào `wwwroot/css/site.css`, Layout nạp file này cho mọi trang.
- CSS chỉ một trang dùng thì tách thành file riêng (`wwwroot/css/bocuc.css`) và nạp qua `@section Styles`.
- Nạp `site.css` **sau** `bootstrap.min.css` để CSS của mình ghi đè được Bootstrap.
- `asp-append-version="true"` thêm `?v=<mã băm>` vào đường dẫn: sửa file CSS thì mã băm đổi, trình duyệt tải bản mới thay vì dùng bản cũ trong cache.
- **CSS isolation**: file `_Layout.cshtml.css` đặt cạnh View, khi build được gộp vào `DemoMVC.styles.css` và chỉ áp dụng cho View đó.

```css
/* wwwroot/css/site.css */
.topic-card { transition: transform .15s, box-shadow .15s; }
.topic-card:hover { transform: translateY(-2px); box-shadow: 0 .5rem 1rem rgba(0,0,0,.08); }
```

```html
<!-- HTML sinh ra -->
<link rel="stylesheet" href="/css/site.css?v=5zG2Yd0Q..." />
```

## 7. Ví dụ trong dự án

| URL | Nội dung |
| --- | --- |
| `/BoCuc` | Lý thuyết về Layout |
| `/BoCuc/ViDuBootstrap` | Thành phần Bootstrap 5, nạp `bocuc.css` qua section `Styles`, JS hiện breakpoint qua section `Scripts` |
| `/BoCuc/LayoutRieng` | Trang dùng `_LayoutDemo.cshtml` thay cho layout mặc định |
