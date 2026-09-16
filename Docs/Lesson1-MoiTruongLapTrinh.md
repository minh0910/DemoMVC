# Lesson 1: Tìm hiểu về môi trường lập trình

> Mục tiêu: tìm hiểu và cài đặt .NET, VS Code và các Extension cần thiết để lập trình ASP.NET Core MVC.

## 1. .NET là gì?

- **.NET** là nền tảng lập trình mã nguồn mở, chạy được trên Windows, macOS và Linux, do Microsoft phát triển. Ngôn ngữ chính là **C#**.
- **ASP.NET Core** là framework để làm web trên .NET. **MVC** là một cách tổ chức ứng dụng web trong ASP.NET Core.
- **.NET SDK** (Software Development Kit): gồm trình biên dịch, công cụ dòng lệnh `dotnet` và thư viện. Máy dùng để **lập trình** thì cần cài SDK.
- **.NET Runtime**: chỉ dùng để **chạy** ứng dụng đã build (ví dụ trên server).
- Các loại phiên bản:
  - **LTS** (Long Term Support): hỗ trợ 3 năm, số phiên bản chẵn, phát hành vào tháng 11 các năm lẻ (.NET 8, .NET 10).
  - **STS** (Standard Term Support): hỗ trợ ngắn hơn, số phiên bản lẻ (.NET 9).
  - Dự án DemoMVC dùng `net10.0`.

## 2. Cài đặt .NET SDK

1. Vào <https://dotnet.microsoft.com/download>, chọn **.NET SDK** bản LTS mới nhất cho hệ điều hành của mình.
2. Chạy file cài đặt (`.exe` trên Windows, `.pkg` trên macOS).
   Trên macOS có thể cài bằng Homebrew: `brew install --cask dotnet-sdk`.
3. Mở Terminal để kiểm tra:

```bash
dotnet --version        # phiên bản SDK đang dùng
dotnet --list-sdks      # các SDK đã cài
dotnet --list-runtimes  # các Runtime đã cài
dotnet --info           # thông tin chi tiết về môi trường
```

## 3. Cài đặt Visual Studio Code

1. Tải tại <https://code.visualstudio.com> và cài đặt.
2. Trên macOS: mở Command Palette (`Cmd` + `Shift` + `P`) → chọn **Shell Command: Install 'code' command in PATH**. Sau đó mở thư mục dự án bằng lệnh `code .`

## 4. Các Extension nên cài

| Extension                                     | Tác dụng                                                                                           |
| --------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| **C# Dev Kit** (Microsoft)                    | Solution Explorer, chạy/debug, quản lý project, chạy test. Tự cài kèm _C#_ và _.NET Install Tool_. |
| **C#** (Microsoft)                            | Tô màu cú pháp, gợi ý code (IntelliSense), báo lỗi, debug.                                         |
| **.NET Install Tool**                         | Tự tải SDK/Runtime mà các extension khác cần.                                                      |
| **IntelliCode for C# Dev Kit**                | Gợi ý code thông minh bằng AI.                                                                     |
| **NuGet Gallery**                             | Tìm và cài thư viện NuGet bằng giao diện.                                                          |
| **Auto Rename Tag**, **Bootstrap 5 Snippets** | Hỗ trợ viết HTML trong file `.cshtml`.                                                             |
| **GitLens**                                   | Xem lịch sử Git ngay trong editor.                                                                 |

## 5. Các lệnh `dotnet` hay dùng

```bash
dotnet new mvc -n DemoMVC     # tạo dự án MVC mới
cd DemoMVC
dotnet new gitignore          # tạo .gitignore chuẩn cho .NET (bỏ qua bin/ obj/)

dotnet restore                # tải các package NuGet
dotnet build                  # biên dịch
dotnet run                    # chạy ứng dụng
dotnet watch                  # chạy và tự tải lại khi sửa code (hot reload)
dotnet add package Ten.Package          # thêm thư viện
dotnet publish -c Release -o ./publish  # đóng gói để triển khai
```

## 6. Kiểm tra môi trường đã sẵn sàng

Chạy `dotnet run` trong thư mục dự án. Nếu terminal in ra dòng kiểu

```
Now listening on: http://localhost:5231
```

và mở được địa chỉ đó trên trình duyệt thì môi trường đã sẵn sàng.
