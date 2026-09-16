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

### 2.0. Hosting và domain của dự án này

| | |
|---|---|
| Nhà cung cấp hosting | **MonsterASP.NET** (gói miễn phí, Windows/IIS, hỗ trợ ASP.NET Core) |
| Domain | **<http://vuleminh.runasp.net/>** |
| Cách deploy | GitHub Actions + FTP |

Các bước đăng ký:
1. Tạo tài khoản tại <https://www.monsterasp.net> và chọn gói miễn phí.
2. Tạo website mới; hệ thống cấp sẵn subdomain miễn phí dạng `<ten>.runasp.net`, ở đây là `vuleminh.runasp.net`.
   Vì là subdomain do hosting cấp nên **không cần mua tên miền và không cần cấu hình DNS**.
3. Trong control panel, mục **Deploy → FTP / SFTP access**, lấy Login và Password FTP để dùng cho CI/CD.
4. Nếu sau này muốn dùng tên miền riêng (ví dụ `vuleminh.com`): mua tên miền, thêm domain vào website trên MonsterASP rồi trỏ DNS theo hướng dẫn bên dưới.

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

### 4.2. Workflow thực tế của dự án: deploy lên vuleminh.runasp.net qua FTP

File `.github/workflows/deploy.yml` đang dùng trong repo:

```yaml
name: Build & Deploy to MonsterASP.NET

# Tự động chạy mỗi khi push lên nhánh main, hoặc bấm chạy tay trong tab Actions
on:
  push:
    branches: [ main ]
  workflow_dispatch:

env:
  FTP_HOST: site84980.siteasp.net
  FTP_DIR: /wwwroot

jobs:
  build_and_deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Lấy mã nguồn
        uses: actions/checkout@v4

      - name: Cài .NET 10 SDK
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Publish
        run: dotnet publish DemoMVC.csproj --configuration Release --output ./publish --runtime win-x86

      - name: Cài lftp
        run: sudo apt-get update -qq && sudo apt-get install -y -qq lftp

      - name: Deploy lên vuleminh.runasp.net qua FTP
        env:
          FTP_USER: ${{ secrets.SERVER_USERNAME }}
          LFTP_PASSWORD: ${{ secrets.SERVER_PASSWORD }}
        run: |
          # app_offline.htm: IIS tạm dừng ứng dụng để ghi đè được các file .dll đang chạy
          echo '<h1>Website đang được cập nhật, vui lòng quay lại sau ít giây...</h1>' > app_offline.htm
          lftp --env-password -u "$FTP_USER" "$FTP_HOST" <<EOF
          set cmd:fail-exit yes
          # FTPS của hosting dùng chứng chỉ thiếu chuỗi CA: vẫn mã hoá nhưng bỏ kiểm tra chứng chỉ
          set ssl:verify-certificate no
          set net:max-retries 3
          set net:timeout 30
          put app_offline.htm -o $FTP_DIR/app_offline.htm
          mirror --reverse --no-perms --verbose --parallel=4 ./publish/ $FTP_DIR/
          rm $FTP_DIR/app_offline.htm
          bye
          EOF
```

Giải thích:
- **Trigger**: chạy khi push lên `main` hoặc bấm *Run workflow* trong tab Actions.
- **Publish** với `--runtime win-x86` vì hosting chạy IIS trên Windows.
- **lftp** upload thư mục `publish/` vào `/wwwroot` (website root trên MonsterASP.NET).
- **app_offline.htm**: upload trước để IIS tạm dừng ứng dụng, tránh lỗi file `.dll` đang bị khoá; upload xong thì xoá để website chạy lại.
- `set ssl:verify-certificate no`: FTPS của hosting dùng chứng chỉ thiếu chuỗi CA, kết nối vẫn mã hoá nhưng bỏ bước kiểm tra chứng chỉ.
- `mirror --no-perms`: server Windows không hỗ trợ `chmod` nên không đặt quyền file.

Secrets cần tạo (lấy trong **Deploy → FTP / SFTP access** của control panel):

| Secret | Giá trị |
|---|---|
| `SERVER_USERNAME` | Login FTP, ví dụ `site84980` |
| `SERVER_PASSWORD` | Mật khẩu FTP |

Các lỗi đã gặp khi thiết lập và cách xử lý:

| Lỗi | Nguyên nhân | Cách xử lý |
|---|---|---|
| `ERROR_USER_UNAUTHORIZED (401)` khi dùng Web Deploy | WebDeploy trong control panel đang **Disabled**, chưa có tài khoản | Bật WebDeploy, hoặc chuyển sang deploy bằng FTP |
| `Certificate verification: The certificate is NOT trusted` | Chứng chỉ FTPS của hosting thiếu chuỗi CA | `set ssl:verify-certificate no` |
| `SITE CHMOD are not supported` | Server Windows không hỗ trợ đặt quyền file | `mirror --no-perms` |
| Push file workflow bị từ chối | Token GitHub thiếu quyền `workflow` | `gh auth refresh -s workflow` |

> Cách khác: nếu bật **WebDeploy** trong control panel, có thể dùng action `rasmusbuchholdt/simply-web-deploy` (chạy trên `windows-latest`) với các secret `WEBSITE_NAME`, `SERVER_COMPUTER_NAME` (`https://site84980.siteasp.net:8172`), `SERVER_USERNAME`, `SERVER_PASSWORD` (mật khẩu WebDeploy).

Sau khi workflow chạy xong, mở <http://vuleminh.runasp.net/> để kiểm tra phiên bản mới.

### 4.3. Phương án deploy lên VPS Linux

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
