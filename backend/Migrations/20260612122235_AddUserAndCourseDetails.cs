using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAndCourseDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Credit",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InstructorId",
                table: "Courses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMandatory",
                table: "Courses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Credit", "InstructorId", "IsMandatory" },
                values: new object[] { 4, 2, true });

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Credit", "InstructorId", "IsMandatory" },
                values: new object[] { 3, 2, true });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { 1, "admin@mudek.edu.tr", "Sistem Yöneticisi", "PLACEHOLDER", "Admin" },
                    { 2, "ali.vural@mudek.edu.tr", "Dr. Ali Vural", "PLACEHOLDER", "Instructor" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_InstructorId",
                table: "Courses",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_InstructorId",
                table: "Courses",
                column: "InstructorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_InstructorId",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Courses_InstructorId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Credit",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "InstructorId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsMandatory",
                table: "Courses");
        }
    }
}
