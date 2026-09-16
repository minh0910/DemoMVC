# Lesson 2: Tìm hiểu về CI/CD

> Mục tiêu: đăng ký hosting và domain, tạo dự án .NET MVC, thiết lập quy trình tự động triển khai dự án.

## 1. CI/CD là gì?

- **CI – Continuous Integration** (tích hợp liên tục): mỗi lần đẩy code lên, hệ thống **tự động build và test**.
- **CD – Continuous Delivery / Deployment** (triển khai liên tục): code đạt yêu cầu được **tự động đưa lên server**.

Quy trình:

```
Viết code → git push → Restore & Build → Test → Publish → Deploy lên hosting → Website chạy tại domain
```

Lợi ích:
- Bỏ các bước làm tay (build, copy file, upload FTP…).
- Phát hiện lỗi sớm, vì code lỗi thì build/test thất bại ngay.
- Lần triển khai nào cũng làm theo đúng một quy trình, nhanh và ít sai sót.

## 2. Đăng ký hosting và domain

### 2.1. Domain (tên miền)

- Là địa chỉ dễ nhớ thay cho địa chỉ IP, ví dụ `demomvc.vn`.
- Mua tại các nhà đăng ký như Mắt Bão, PA Vietnam, Namecheap, Cloudflare… Tên miền phải gia hạn theo năm.
- Sau khi mua cần cấu hình **DNS**:
  - **Bản ghi A**: trỏ `demomvc.vn` → địa chỉ IP của hosting.
  - **Bản ghi CNAME**: trỏ `www` → `demomvc.vn`.
  - Hoặc đổi **Nameserver** sang nameserver của nhà cung cấp hosting.
- DNS có thể mất từ vài phút đến 24–48 giờ mới cập nhật xong.

### 2.2. Hosting

| Loại | Đặc điểm | Cách deploy phổ biến |
|---|---|---|
| **Shared hosting Windows** (Plesk) | Rẻ, có sẵn IIS hỗ trợ ASP.NET Core, ít quyền cấu hình | FTP / Web Deploy |
| **VPS Linux** | Toàn quyền quản trị, tự cài .NET Runtime và Nginx làm reverse proxy | SSH + scp/rsync, chạy app bằng systemd |
| **Cloud PaaS** (Azure App Service…) | Không phải quản lý server, tự mở rộng | Có sẵn action cho GitHub Actions |

Khi chọn shared hosting cần kiểm tra:
- Có hỗ trợ đúng phiên bản .NET của dự án không (ở đây là .NET 10).
- Có SSL miễn phí (Let's Encrypt) không.
- Có tài khoản FTP không.

## 3. Tạo dự án .NET MVC và đưa lên GitHub

```bash
dotnet new mvc -n DemoMVC
cd DemoMVC
dotnet new gitignore          # không đưa bin/ obj/ lên Git
git init
git add .
git commit -m "Initial ASP.NET Core MVC project"
git branch -M main
git remote add origin https://github.com/<user>/DemoMVC.git
git push -u origin main
```

## 4. Thiết lập tự động triển khai bằng GitHub Actions

Tạo file `.github/workflows/deploy.yml`. Mỗi lần push lên nhánh `main`, GitHub sẽ tự build, publish và upload lên hosting qua FTP.

```yaml
name: Build & Deploy DemoMVC

on:
  push:
    branches: [ main ]
  workflow_dispatch:            # cho phép bấm chạy bằng tay

jobs:
  build-deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Lấy mã nguồn
        uses: actions/checkout@v4

      - name: Cài .NET SDK
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build -c Release --no-restore

      - name: Test
        run: dotnet test -c Release --no-build   # nếu có project test

      - name: Publish
        run: dotnet publish -c Release -o ./publish --no-build

      - name: Deploy qua FTP
        uses: SamKirkland/FTP-Deploy-Action@v4.3.5
        with:
          server: ${{ secrets.FTP_SERVER }}
          username: ${{ secrets.FTP_USERNAME }}
          password: ${{ secrets.FTP_PASSWORD }}
          local-dir: ./publish/
          server-dir: /httpdocs/
```

### 4.1. Cấu hình Secrets

Trên GitHub vào **Settings → Secrets and variables → Actions → New repository secret**, thêm:

- `FTP_SERVER`
- `FTP_USERNAME`
- `FTP_PASSWORD`

Không bao giờ ghi mật khẩu trực tiếp vào file YAML.

### 4.2. Phương án deploy lên VPS Linux

Thay bước "Deploy qua FTP" bằng:

```yaml
      - name: Copy lên VPS
        uses: appleboy/scp-action@v0.1.7
        with:
          host: ${{ secrets.VPS_HOST }}
          username: ${{ secrets.VPS_USER }}
          key: ${{ secrets.VPS_SSH_KEY }}
          source: "publish/*"
          target: "/var/www/demomvc"
          strip_components: 1

      - name: Khởi động lại service
        uses: appleboy/ssh-action@v1.0.3
        with:
          host: ${{ secrets.VPS_HOST }}
          username: ${{ secrets.VPS_USER }}
          key: ${{ secrets.VPS_SSH_KEY }}
          script: sudo systemctl restart demomvc
```

## 5. Lưu ý

- Với **IIS / shared hosting**, file `.dll` đang chạy có thể bị khoá, không ghi đè được. Cách xử lý: upload file `app_offline.htm` trước khi deploy để IIS tạm dừng ứng dụng, deploy xong thì xoá file này.
- Xem kết quả mỗi lần chạy workflow ở tab **Actions** của repository. Bước nào lỗi sẽ hiện dấu ❌ kèm log chi tiết.
