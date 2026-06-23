using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseContentApprovalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Courses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentStatus",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "Courses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByUserId",
                table: "Courses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "Courses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApprovedAt", "ContentStatus", "ReviewNote", "ReviewedByUserId", "SubmittedAt" },
                values: new object[] { null, "Draft", null, null, null });

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApprovedAt", "ContentStatus", "ReviewNote", "ReviewedByUserId", "SubmittedAt" },
                values: new object[] { null, "Draft", null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ReviewedByUserId",
                table: "Courses",
                column: "ReviewedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_ReviewedByUserId",
                table: "Courses",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_ReviewedByUserId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_ReviewedByUserId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ContentStatus",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "Courses");
        }
    }
}
