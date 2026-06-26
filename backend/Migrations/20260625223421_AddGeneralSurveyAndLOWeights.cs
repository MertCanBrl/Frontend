using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneralSurveyAndLOWeights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseSurveyQuestions_LearningOutcomes_LearningOutcomeId",
                table: "CourseSurveyQuestions");

            migrationBuilder.DropIndex(
                name: "IX_CourseSurveyQuestions_LearningOutcomeId",
                table: "CourseSurveyQuestions");

            migrationBuilder.DropColumn(
                name: "LearningOutcomeId",
                table: "CourseSurveyQuestions");

            migrationBuilder.CreateTable(
                name: "GeneralSurveyQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OrderNumber = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralSurveyQuestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SurveyQuestionLOWeights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SurveyQuestionId = table.Column<int>(type: "integer", nullable: false),
                    LearningOutcomeId = table.Column<int>(type: "integer", nullable: false),
                    WeightPercentage = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyQuestionLOWeights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyQuestionLOWeights_CourseSurveyQuestions_SurveyQuestio~",
                        column: x => x.SurveyQuestionId,
                        principalTable: "CourseSurveyQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SurveyQuestionLOWeights_LearningOutcomes_LearningOutcomeId",
                        column: x => x.LearningOutcomeId,
                        principalTable: "LearningOutcomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "GeneralSurveyQuestions",
                columns: new[] { "Id", "IsActive", "OrderNumber", "QuestionText" },
                values: new object[,]
                {
                    { 1, true, 1, "Dersin öğretim üyesi derse iyi hazırlanmış olarak gelmektedir." },
                    { 2, true, 2, "Dersin öğretim üyesinin dersi anlatma yeterliliği hakkında ne düşünüyorsunuz?" },
                    { 3, true, 3, "Dersin öğretim üyesi ders saatlerine uymaktadır." },
                    { 4, true, 4, "Öğretim üyesi, ders içeriğiyle ilgili araştırma yapmayı teşvik etmekte ve öğrenci katılımını sağlamaktadır." },
                    { 5, true, 5, "Öğretim üyesi, öğrencilerin öğrenme düzeyini değerlendirme ve ölçmede ne ölçüde başarılıdır?" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SurveyQuestionLOWeights_LearningOutcomeId",
                table: "SurveyQuestionLOWeights",
                column: "LearningOutcomeId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyQuestionLOWeights_SurveyQuestionId_LearningOutcomeId",
                table: "SurveyQuestionLOWeights",
                columns: new[] { "SurveyQuestionId", "LearningOutcomeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralSurveyQuestions");

            migrationBuilder.DropTable(
                name: "SurveyQuestionLOWeights");

            migrationBuilder.AddColumn<int>(
                name: "LearningOutcomeId",
                table: "CourseSurveyQuestions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseSurveyQuestions_LearningOutcomeId",
                table: "CourseSurveyQuestions",
                column: "LearningOutcomeId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSurveyQuestions_LearningOutcomes_LearningOutcomeId",
                table: "CourseSurveyQuestions",
                column: "LearningOutcomeId",
                principalTable: "LearningOutcomes",
                principalColumn: "Id");
        }
    }
}
