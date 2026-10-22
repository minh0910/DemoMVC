using Microsoft.EntityFrameworkCore;
using DemoMVC.Models;

namespace DemoMVC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
// Cấu hình precision cho Price tại đây
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // ===== Khoá ngoại Khoa - Ngành - Sinh viên =====
            // Restrict: không cho xoá Khoa/Ngành khi vẫn còn dữ liệu tham chiếu tới nó
            // (đồng thời tránh lỗi "multiple cascade paths" của SQL Server vì Sinh viên trỏ tới Khoa theo 2 đường)

            // Ngành thuộc 1 khoa: Majors.FacultyCode -> Faculties.FacultyCode
            modelBuilder.Entity<Major>()
                .HasOne(m => m.Faculty)
                .WithMany(f => f.Majors)
                .HasForeignKey(m => m.FacultyCode)
                .OnDelete(DeleteBehavior.Restrict);

            // Sinh viên thuộc 1 khoa: Students.FacultyCode -> Faculties.FacultyCode
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Faculty)
                .WithMany(f => f.Students)
                .HasForeignKey(s => s.FacultyCode)
                .OnDelete(DeleteBehavior.Restrict);

            // Sinh viên học 1 ngành: Students.MajorCode -> Majors.MajorCode
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Major)
                .WithMany(m => m.Students)
                .HasForeignKey(s => s.MajorCode)
                .OnDelete(DeleteBehavior.Restrict);

            // Dữ liệu mẫu: các khoa của Trường Đại học Mỏ - Địa chất, ngành và 1 sinh viên ví dụ
            modelBuilder.Entity<Faculty>().HasData(SeedData.Faculties);
            modelBuilder.Entity<Major>().HasData(SeedData.Majors);
            modelBuilder.Entity<Student>().HasData(SeedData.Students);
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<Faculty> Faculties { get; set; }

        public DbSet<Major> Majors { get; set; }

        public DbSet<Student> Students { get; set; }
    }
}
