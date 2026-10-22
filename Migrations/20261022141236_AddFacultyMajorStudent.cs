using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DemoMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddFacultyMajorStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Faculties",
                columns: table => new
                {
                    FacultyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faculties", x => x.FacultyCode);
                });

            migrationBuilder.CreateTable(
                name: "Majors",
                columns: table => new
                {
                    MajorCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FacultyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Majors", x => x.MajorCode);
                    table.ForeignKey(
                        name: "FK_Majors_Faculties_FacultyCode",
                        column: x => x.FacultyCode,
                        principalTable: "Faculties",
                        principalColumn: "FacultyCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EnrollmentYear = table.Column<int>(type: "int", nullable: false),
                    TrainingSystemCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    FacultyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MajorCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentCode);
                    table.ForeignKey(
                        name: "FK_Students_Faculties_FacultyCode",
                        column: x => x.FacultyCode,
                        principalTable: "Faculties",
                        principalColumn: "FacultyCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Students_Majors_MajorCode",
                        column: x => x.MajorCode,
                        principalTable: "Majors",
                        principalColumn: "MajorCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Faculties",
                columns: new[] { "FacultyCode", "Name" },
                values: new object[,]
                {
                    { "CD", "Khoa Cơ điện" },
                    { "CNTT", "Khoa Công nghệ thông tin" },
                    { "DKNL", "Khoa Dầu khí và Năng lượng" },
                    { "GDQP", "Khoa Giáo dục quốc phòng" },
                    { "KHCB", "Khoa Khoa học cơ bản" },
                    { "KHKTDC", "Khoa Khoa học và Kỹ thuật Địa chất" },
                    { "KTQTKD", "Khoa Kinh tế - Quản trị kinh doanh" },
                    { "LLCT", "Khoa Lý luận chính trị" },
                    { "MO", "Khoa Mỏ" },
                    { "MT", "Khoa Môi trường" },
                    { "TDBD", "Khoa Trắc địa - Bản đồ và Quản lý đất đai" },
                    { "XD", "Khoa Xây dựng" }
                });

            migrationBuilder.InsertData(
                table: "Majors",
                columns: new[] { "MajorCode", "FacultyCode", "Name" },
                values: new object[,]
                {
                    { "01", "MO", "Kỹ thuật mỏ" },
                    { "02", "MO", "Kỹ thuật tuyển khoáng" },
                    { "03", "DKNL", "Kỹ thuật dầu khí" },
                    { "04", "KHKTDC", "Kỹ thuật địa chất" },
                    { "05", "CNTT", "Công nghệ thông tin" },
                    { "06", "CNTT", "Khoa học dữ liệu" },
                    { "07", "TDBD", "Kỹ thuật trắc địa - bản đồ" },
                    { "08", "TDBD", "Quản lý đất đai" },
                    { "09", "CD", "Kỹ thuật điện" },
                    { "10", "CD", "Kỹ thuật cơ điện tử" },
                    { "11", "XD", "Kỹ thuật xây dựng" },
                    { "12", "MT", "Kỹ thuật môi trường" },
                    { "13", "KTQTKD", "Kế toán" },
                    { "14", "KTQTKD", "Quản trị kinh doanh" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentCode", "DateOfBirth", "Email", "EnrollmentYear", "FacultyCode", "FullName", "MajorCode", "TrainingSystemCode" },
                values: new object[] { "2821050199", new DateTime(2010, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2028, "CNTT", "Nguyễn Văn An", "05", "21" });

            migrationBuilder.CreateIndex(
                name: "IX_Majors_FacultyCode",
                table: "Majors",
                column: "FacultyCode");

            migrationBuilder.CreateIndex(
                name: "IX_Students_FacultyCode",
                table: "Students",
                column: "FacultyCode");

            migrationBuilder.CreateIndex(
                name: "IX_Students_MajorCode",
                table: "Students",
                column: "MajorCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Majors");

            migrationBuilder.DropTable(
                name: "Faculties");
        }
    }
}
