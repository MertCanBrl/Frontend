using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseAndEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Semester = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    Midterm = table.Column<decimal>(type: "numeric", nullable: true),
                    Final = table.Column<decimal>(type: "numeric", nullable: true),
                    MakeUp = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enrollments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "Code", "Name", "Semester" },
                values: new object[,]
                {
                    { 1, "BIL101", "Programlamaya Giriş", "2024-Güz" },
                    { 2, "BIL202", "Veri Yapıları", "2024-Güz" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "Id", "Email", "EnrollmentDate", "FirstName", "IsActive", "LastName", "StudentNumber" },
                values: new object[,]
                {
                    { 1, "ahmet.yilmaz@ogr.edu.tr", new DateTime(2021, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Ahmet", true, "Yılmaz", "2021001" },
                    { 2, "ayse.kaya@ogr.edu.tr", new DateTime(2021, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Ayşe", true, "Kaya", "2021002" },
                    { 3, "mehmet.demir@ogr.edu.tr", new DateTime(2022, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Mehmet", true, "Demir", "2022015" }
                });

            migrationBuilder.InsertData(
                table: "Enrollments",
                columns: new[] { "Id", "CourseId", "Final", "MakeUp", "Midterm", "StudentId" },
                values: new object[,]
                {
                    { 1, 1, 80m, null, 70m, 1 },
                    { 2, 1, 55m, 65m, 60m, 2 },
                    { 3, 1, null, null, null, 3 },
                    { 4, 2, 85m, null, 90m, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseId",
                table: "Enrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId_CourseId",
                table: "Enrollments",
                columns: new[] { "StudentId", "CourseId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
