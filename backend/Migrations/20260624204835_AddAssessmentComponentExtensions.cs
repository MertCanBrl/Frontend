using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentComponentExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "WeightPercentage",
                table: "Exams",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradeGroup",
                table: "AssessmentComponents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GroupWeightPercentage",
                table: "AssessmentComponents",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsIncludedInAverage",
                table: "AssessmentComponents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxScore",
                table: "AssessmentComponents",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "AssessmentComponentLearningOutcomes",
                columns: table => new
                {
                    AssessmentComponentId = table.Column<int>(type: "integer", nullable: false),
                    LearningOutcomeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentComponentLearningOutcomes", x => new { x.AssessmentComponentId, x.LearningOutcomeId });
                    table.ForeignKey(
                        name: "FK_AssessmentComponentLearningOutcomes_AssessmentComponents_As~",
                        column: x => x.AssessmentComponentId,
                        principalTable: "AssessmentComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentComponentLearningOutcomes_LearningOutcomes_Learni~",
                        column: x => x.LearningOutcomeId,
                        principalTable: "LearningOutcomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentComponentStudentGrades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssessmentComponentId = table.Column<int>(type: "integer", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentComponentStudentGrades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentComponentStudentGrades_AssessmentComponents_Asses~",
                        column: x => x.AssessmentComponentId,
                        principalTable: "AssessmentComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentComponentStudentGrades_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentComponentLearningOutcomes_LearningOutcomeId",
                table: "AssessmentComponentLearningOutcomes",
                column: "LearningOutcomeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentComponentStudentGrades_AssessmentComponentId_Stud~",
                table: "AssessmentComponentStudentGrades",
                columns: new[] { "AssessmentComponentId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentComponentStudentGrades_StudentId",
                table: "AssessmentComponentStudentGrades",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentComponentLearningOutcomes");

            migrationBuilder.DropTable(
                name: "AssessmentComponentStudentGrades");

            migrationBuilder.DropColumn(
                name: "WeightPercentage",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "GradeGroup",
                table: "AssessmentComponents");

            migrationBuilder.DropColumn(
                name: "GroupWeightPercentage",
                table: "AssessmentComponents");

            migrationBuilder.DropColumn(
                name: "IsIncludedInAverage",
                table: "AssessmentComponents");

            migrationBuilder.DropColumn(
                name: "MaxScore",
                table: "AssessmentComponents");
        }
    }
}
