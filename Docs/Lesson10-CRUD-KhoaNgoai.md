# Lesson 10: Tiếp tục CRUD với bảng có khoá ngoại

> Ví dụ chạy thử: các trang `/Faculty` (Khoa), `/Major` (Ngành), `/Student` (Sinh viên).
> Code ở `Models/Faculty.cs`, `Models/Major.cs`, `Models/Student.cs`, `Models/TrainingSystems.cs`, `Data/ApplicationDbContext.cs`, `Data/SeedData.cs`, `Services/StudentCodeGenerator.cs`, `Controllers/FacultyController.cs`, `Controllers/MajorController.cs`, `Controllers/StudentController.cs` và `Views/Faculty|Major|Student/`.

Nội dung:

- Tạo liên kết khoá ngoại: bảng Sinh viên và bảng Khoa liên kết qua **mã khoa**.
- Nhập dữ liệu các khoa của **Trường Đại học Mỏ - Địa chất**.
- Xây dựng chức năng CRUD với các bảng có khoá ngoại.
- Sinh mã sinh viên tự động, ví dụ `2821050199`, sinh viên tiếp theo có mã `2821050200`.

## 1. Thiết kế CSDL

```
Faculties (Khoa)               Majors (Ngành)                  Students (Sinh viên)
+---------------------+        +----------------------+        +--------------------------+
| PK FacultyCode      |<--+----| FK FacultyCode       |   +----| FK MajorCode             |
|    Name             |   |    | PK MajorCode         |<--+    | PK StudentCode           |
+---------------------+   |    |    Name              |        |    FullName, DateOfBirth |
                          |    +----------------------+        |    Email, EnrollmentYear |
                          |                                    |    TrainingSystemCode    |
                          +------------------------------------| FK FacultyCode           |
                                                               +--------------------------+
```

| Bảng | Khoá chính | Khoá ngoại |
| --- | --- | --- |
| `Faculties` | `FacultyCode` (ví dụ `CNTT`) | |
| `Majors` | `MajorCode` (2 chữ số, ví dụ `05`) | `FacultyCode` → `Faculties` |
| `Students` | `StudentCode` (10 chữ số, sinh tự động) | `FacultyCode` → `Faculties`, `MajorCode` → `Majors` |

- **Khoá chính (Primary Key)**: định danh duy nhất mỗi dòng. Ở đây dùng mã có ý nghĩa (`CNTT`, `05`, `2821050199`) thay vì số tự tăng.
- **Khoá ngoại (Foreign Key)**: cột chứa giá trị khoá chính của bảng khác. CSDL kiểm tra giá trị này phải tồn tại ở bảng được tham chiếu: không thêm được sinh viên với mã khoa `ABC` nếu bảng Khoa không có `ABC`.
- Hệ đào tạo chỉ có vài giá trị cố định nên để thành danh mục trong code (`Models/TrainingSystems.cs`), không tạo bảng.

## 2. Khai báo Entity và navigation property

```csharp
public class Faculty
{
    [Key]
    [StringLength(10)]
    public string FacultyCode { get; set; } = string.Empty;   // khoá chính

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Major> Majors { get; set; } = new List<Major>();      // 1 khoa - nhiều ngành
    public ICollection<Student> Students { get; set; } = new List<Student>(); // 1 khoa - nhiều sinh viên
}

public class Student
{
    [Key, StringLength(10)]
    public string StudentCode { get; set; } = string.Empty;

    [Required, StringLength(10)]
    public string FacultyCode { get; set; } = string.Empty;   // cột khoá ngoại
    public Faculty? Faculty { get; set; }                     // navigation: đối tượng Khoa tương ứng

    [Required, StringLength(2)]
    public string MajorCode { get; set; } = string.Empty;
    public Major? Major { get; set; }
    // ...
}
```

- **Navigation property** (`Faculty`, `Majors`, `Students`) giúp đi từ bảng này sang bảng kia bằng code C#, ví dụ `student.Faculty.Name`.
- Navigation để kiểu nullable (`Faculty?`) để model binding không bắt buộc form phải gửi lên cả đối tượng Khoa.

## 3. Cấu hình khoá ngoại bằng Fluent API

Vì tên khoá không theo quy ước `Id`/`<Tên>Id`, ta khai báo rõ trong `OnModelCreating`:

```csharp
modelBuilder.Entity<Student>()
    .HasOne(s => s.Faculty)            // mỗi sinh viên có 1 khoa
    .WithMany(f => f.Students)         // mỗi khoa có nhiều sinh viên
    .HasForeignKey(s => s.FacultyCode) // qua cột FacultyCode
    .OnDelete(DeleteBehavior.Restrict);
```

`DeleteBehavior` quyết định điều gì xảy ra khi xoá dòng được tham chiếu:

| Giá trị | Ý nghĩa |
| --- | --- |
| `Cascade` | Xoá Khoa thì xoá luôn các Ngành/Sinh viên thuộc khoa |
| `Restrict` / `NoAction` | Không cho xoá Khoa khi còn dòng tham chiếu tới (dự án dùng cách này) |
| `SetNull` | Xoá Khoa thì đặt khoá ngoại về `NULL` (cột phải cho phép null) |

Dùng `Restrict` còn để tránh lỗi *multiple cascade paths* của SQL Server: sinh viên trỏ tới Khoa theo 2 đường (trực tiếp và qua Ngành), SQL Server không cho cả 2 đường cùng `Cascade`.

## 4. Nhập dữ liệu các khoa bằng HasData

Dữ liệu mẫu nằm trong `Data/SeedData.cs`, đưa vào model bằng `HasData`. Khi tạo migration, EF sinh lệnh `InsertData` nên chạy migration là có dữ liệu.

```csharp
modelBuilder.Entity<Faculty>().HasData(SeedData.Faculties);
modelBuilder.Entity<Major>().HasData(SeedData.Majors);
modelBuilder.Entity<Student>().HasData(SeedData.Students);
```

12 khoa của Trường Đại học Mỏ - Địa chất (theo humg.edu.vn, mục *Cơ cấu tổ chức → Các khoa*):

| Mã | Tên khoa |
| --- | --- |
| CNTT | Khoa Công nghệ thông tin |
| CD | Khoa Cơ điện |
| DKNL | Khoa Dầu khí và Năng lượng |
| KTQTKD | Khoa Kinh tế - Quản trị kinh doanh |
| KHKTDC | Khoa Khoa học và Kỹ thuật Địa chất |
| MO | Khoa Mỏ |
| MT | Khoa Môi trường |
| TDBD | Khoa Trắc địa - Bản đồ và Quản lý đất đai |
| XD | Khoa Xây dựng |
| KHCB | Khoa Khoa học cơ bản |
| LLCT | Khoa Lý luận chính trị |
| GDQP | Khoa Giáo dục quốc phòng |

Mã khoa là chữ viết tắt tự đặt. Ngành có 14 ngành tiêu biểu (`01`–`14`); mã `05` = Công nghệ thông tin và hệ `21` = Đại học chính quy lấy theo đề bài, các mã còn lại là ví dụ. Có sẵn 1 sinh viên mẫu `2821050199` để thử ví dụ của đề bài.

Tạo migration và cập nhật CSDL:

```bash
dotnet ef migrations add AddFacultyMajorStudent
dotnet ef database update        # máy local; trên hosting thì app tự chạy Database.Migrate() khi khởi động
```

## 5. CRUD với bảng có khoá ngoại

### 5.1. Đọc dữ liệu kèm bảng liên quan: `Include`

```csharp
var students = await _context.Students
    .Include(s => s.Faculty)   // JOIN Faculties
    .Include(s => s.Major)     // JOIN Majors
    .ToListAsync();
```

```cshtml
<td>@item.Faculty?.Name</td>   @* hiện tên khoa thay vì mã khoa *@
```

Không có `Include` thì `item.Faculty` là `null` (EF không tự lấy bảng liên quan).

### 5.2. Chọn khoá ngoại bằng dropdown: `SelectList`

```csharp
ViewBag.FacultyList = new SelectList(faculties, "FacultyCode", "Name", selectedValue);
```

```cshtml
<select asp-for="FacultyCode" asp-items="ViewBag.FacultyList" class="form-select">
    <option value="">-- Chọn khoa --</option>
</select>
```

Người dùng nhìn thấy tên khoa, form gửi lên mã khoa.

Ở form thêm sinh viên, dropdown **Ngành** được lọc theo **Khoa** đã chọn bằng JavaScript (danh sách ngành kèm mã khoa được gửi xuống trang bằng `@Json.Serialize`). Server vẫn kiểm tra lại ngành có thuộc đúng khoa không, vì dữ liệu từ trình duyệt có thể bị sửa.

### 5.3. Quy tắc khi sửa, xoá

| Chức năng | Quy tắc |
| --- | --- |
| Sửa Khoa | Chỉ sửa tên. Mã khoa là khoá chính đang được tham chiếu nên không sửa |
| Xoá Khoa | Chặn nếu còn ngành hoặc sinh viên thuộc khoa |
| Sửa Ngành | Sửa tên, đổi khoa; chặn đổi khoa nếu ngành đã có sinh viên |
| Xoá Ngành | Chặn nếu còn sinh viên thuộc ngành |
| Sửa Sinh viên | Chỉ sửa họ tên, ngày sinh, email. Năm nhập học, hệ, khoa, ngành đã nằm trong mã sinh viên |

Khi sửa, dùng `TryUpdateModelAsync` với danh sách thuộc tính được phép để chống **overposting** (người dùng tự thêm field vào form để sửa những cột không được phép):

```csharp
var student = await _context.Students.FindAsync(id);
if (await TryUpdateModelAsync(student, "", s => s.FullName, s => s.DateOfBirth, s => s.Email))
{
    await _context.SaveChangesAsync();
}
```

## 6. Sinh mã sinh viên tự động

Mã gồm 10 chữ số, ví dụ `2821050199`:

| Vị trí | Giá trị | Ý nghĩa |
| --- | --- | --- |
| 1–2 | `28` | 2 số cuối năm nhập học (2028) |
| 3–4 | `21` | Hệ đào tạo (đại học chính quy) |
| 5–6 | `05` | Ngành học (Công nghệ thông tin) |
| 7–10 | `0199` | Số thứ tự của sinh viên trong cùng năm, hệ, ngành |

Thuật toán (`Services/StudentCodeGenerator.cs`):

1. Ghép tiền tố 6 chữ số: `prefix = $"{nam % 100:D2}{maHe}{maNganh}"` → `282105`.
2. Tìm mã lớn nhất đang bắt đầu bằng tiền tố đó. Các mã cùng tiền tố dài bằng nhau nên so sánh chuỗi cũng là so sánh số.
3. Lấy 4 số cuối + 1, thêm số 0 ở đầu cho đủ 4 chữ số. Chưa có ai thì bắt đầu từ `0001`.

```csharp
var lastCode = await _context.Students
    .Where(s => s.StudentCode.StartsWith(prefix))     // SQL: LIKE N'282105%'
    .OrderByDescending(s => s.StudentCode)
    .Select(s => s.StudentCode)
    .FirstOrDefaultAsync();                           // "2821050199"

var next = lastCode == null ? 1 : int.Parse(lastCode.Substring(prefix.Length)) + 1;   // 200
return prefix + next.ToString("D4");                                                  // "2821050200"
```

- Service đăng ký trong `Program.cs`: `builder.Services.AddScoped<StudentCodeGenerator>();` và được inject vào `StudentController`.
- Mã **chỉ sinh ở server** khi lưu; action `Create` không nhận `StudentCode` từ form (`[Bind]` không có field này), nên người dùng không tự đặt mã được.
- Form thêm mới gọi `GET /Student/NextCode?enrollmentYear=2028&trainingSystemCode=21&majorCode=05` để hiện **mã dự kiến** khi người dùng chọn năm, hệ, ngành.
- Hai người thêm cùng lúc có thể lấy trùng mã; khoá chính sẽ báo lỗi trùng và controller thử sinh lại mã (tối đa 3 lần).

Thử ví dụ của đề bài: vào `/Student/Create`, chọn năm 2028, hệ 21, khoa Công nghệ thông tin, ngành 05 → mã dự kiến `2821050200`. Lưu xong, thêm tiếp sinh viên cùng năm, hệ, ngành sẽ được `2821050201`.
