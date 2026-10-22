using DemoMVC.Models;

namespace DemoMVC.Data
{
    // Dữ liệu mẫu, được đưa vào CSDL qua HasData trong ApplicationDbContext (nằm trong migration)
    public static class SeedData
    {
        // 12 khoa của Trường Đại học Mỏ - Địa chất (theo humg.edu.vn, mục Cơ cấu tổ chức > Các khoa)
        public static readonly Faculty[] Faculties =
        {
            new Faculty { FacultyCode = "CNTT",   Name = "Khoa Công nghệ thông tin" },
            new Faculty { FacultyCode = "CD",     Name = "Khoa Cơ điện" },
            new Faculty { FacultyCode = "DKNL",   Name = "Khoa Dầu khí và Năng lượng" },
            new Faculty { FacultyCode = "KTQTKD", Name = "Khoa Kinh tế - Quản trị kinh doanh" },
            new Faculty { FacultyCode = "KHKTDC", Name = "Khoa Khoa học và Kỹ thuật Địa chất" },
            new Faculty { FacultyCode = "MO",     Name = "Khoa Mỏ" },
            new Faculty { FacultyCode = "MT",     Name = "Khoa Môi trường" },
            new Faculty { FacultyCode = "TDBD",   Name = "Khoa Trắc địa - Bản đồ và Quản lý đất đai" },
            new Faculty { FacultyCode = "XD",     Name = "Khoa Xây dựng" },
            new Faculty { FacultyCode = "KHCB",   Name = "Khoa Khoa học cơ bản" },
            new Faculty { FacultyCode = "LLCT",   Name = "Khoa Lý luận chính trị" },
            new Faculty { FacultyCode = "GDQP",   Name = "Khoa Giáo dục quốc phòng" },
        };

        // Một số ngành tiêu biểu của từng khoa. Mã 05 = Công nghệ thông tin theo đề bài, các mã còn lại là ví dụ.
        public static readonly Major[] Majors =
        {
            new Major { MajorCode = "01", Name = "Kỹ thuật mỏ",                FacultyCode = "MO" },
            new Major { MajorCode = "02", Name = "Kỹ thuật tuyển khoáng",      FacultyCode = "MO" },
            new Major { MajorCode = "03", Name = "Kỹ thuật dầu khí",           FacultyCode = "DKNL" },
            new Major { MajorCode = "04", Name = "Kỹ thuật địa chất",          FacultyCode = "KHKTDC" },
            new Major { MajorCode = "05", Name = "Công nghệ thông tin",        FacultyCode = "CNTT" },
            new Major { MajorCode = "06", Name = "Khoa học dữ liệu",           FacultyCode = "CNTT" },
            new Major { MajorCode = "07", Name = "Kỹ thuật trắc địa - bản đồ", FacultyCode = "TDBD" },
            new Major { MajorCode = "08", Name = "Quản lý đất đai",            FacultyCode = "TDBD" },
            new Major { MajorCode = "09", Name = "Kỹ thuật điện",              FacultyCode = "CD" },
            new Major { MajorCode = "10", Name = "Kỹ thuật cơ điện tử",        FacultyCode = "CD" },
            new Major { MajorCode = "11", Name = "Kỹ thuật xây dựng",          FacultyCode = "XD" },
            new Major { MajorCode = "12", Name = "Kỹ thuật môi trường",        FacultyCode = "MT" },
            new Major { MajorCode = "13", Name = "Kế toán",                    FacultyCode = "KTQTKD" },
            new Major { MajorCode = "14", Name = "Quản trị kinh doanh",        FacultyCode = "KTQTKD" },
        };

        // Sinh viên mẫu theo ví dụ của đề bài: thêm sinh viên năm 2028, hệ 21, ngành 05 sẽ được mã 2821050200
        public static readonly Student[] Students =
        {
            new Student
            {
                StudentCode = "2821050199",
                FullName = "Nguyễn Văn An",
                DateOfBirth = new DateTime(2010, 5, 15),
                EnrollmentYear = 2028,
                TrainingSystemCode = "21",
                FacultyCode = "CNTT",
                MajorCode = "05",
            },
        };
    }
}
