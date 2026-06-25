using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningOutcomeWeightsToAssessmentMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "WeightPercentage",
                table: "ExamQuestionLearningOutcomes",
                type: "numeric",
                nullable: false,
                defaultValue: 100m);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightPercentage",
                table: "AssessmentComponentLearningOutcomes",
                type: "numeric",
                nullable: false,
                defaultValue: 100m);

            // Eşit dağılım: tek ÖÇ → 100, iki ÖÇ → 50/50, üç ÖÇ → 33.33/33.33/33.34, ...
            // Adım 1: ROUND(100 / toplam_sayı, 2) ile eşit dağıt
            migrationBuilder.Sql(@"
                UPDATE ""ExamQuestionLearningOutcomes"" eqlo
                SET ""WeightPercentage"" = ROUND(100.0 / cnt.total, 2)
                FROM (
                    SELECT ""ExamQuestionId"", COUNT(*) AS total
                    FROM ""ExamQuestionLearningOutcomes""
                    GROUP BY ""ExamQuestionId""
                ) cnt
                WHERE eqlo.""ExamQuestionId"" = cnt.""ExamQuestionId"";
            ");
            migrationBuilder.Sql(@"
                UPDATE ""AssessmentComponentLearningOutcomes"" aclo
                SET ""WeightPercentage"" = ROUND(100.0 / cnt.total, 2)
                FROM (
                    SELECT ""AssessmentComponentId"", COUNT(*) AS total
                    FROM ""AssessmentComponentLearningOutcomes""
                    GROUP BY ""AssessmentComponentId""
                ) cnt
                WHERE aclo.""AssessmentComponentId"" = cnt.""AssessmentComponentId"";
            ");
            // Adım 2: Yuvarlama artığını son kayıda ekle (toplam kesinlikle 100 olsun)
            migrationBuilder.Sql(@"
                WITH ranked AS (
                    SELECT ""ExamQuestionId"", ""LearningOutcomeId"", ""WeightPercentage"",
                           ROW_NUMBER() OVER (PARTITION BY ""ExamQuestionId"" ORDER BY ""LearningOutcomeId"" DESC) AS rn,
                           SUM(""WeightPercentage"") OVER (PARTITION BY ""ExamQuestionId"") AS total_w
                    FROM ""ExamQuestionLearningOutcomes""
                ),
                fixups AS (
                    SELECT ""ExamQuestionId"", ""LearningOutcomeId"",
                           100 - total_w + ""WeightPercentage"" AS new_w
                    FROM ranked
                    WHERE rn = 1 AND ABS(total_w - 100) > 0.001
                )
                UPDATE ""ExamQuestionLearningOutcomes"" t
                SET ""WeightPercentage"" = f.new_w
                FROM fixups f
                WHERE t.""ExamQuestionId"" = f.""ExamQuestionId""
                  AND t.""LearningOutcomeId"" = f.""LearningOutcomeId"";
            ");
            migrationBuilder.Sql(@"
                WITH ranked AS (
                    SELECT ""AssessmentComponentId"", ""LearningOutcomeId"", ""WeightPercentage"",
                           ROW_NUMBER() OVER (PARTITION BY ""AssessmentComponentId"" ORDER BY ""LearningOutcomeId"" DESC) AS rn,
                           SUM(""WeightPercentage"") OVER (PARTITION BY ""AssessmentComponentId"") AS total_w
                    FROM ""AssessmentComponentLearningOutcomes""
                ),
                fixups AS (
                    SELECT ""AssessmentComponentId"", ""LearningOutcomeId"",
                           100 - total_w + ""WeightPercentage"" AS new_w
                    FROM ranked
                    WHERE rn = 1 AND ABS(total_w - 100) > 0.001
                )
                UPDATE ""AssessmentComponentLearningOutcomes"" t
                SET ""WeightPercentage"" = f.new_w
                FROM fixups f
                WHERE t.""AssessmentComponentId"" = f.""AssessmentComponentId""
                  AND t.""LearningOutcomeId"" = f.""LearningOutcomeId"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeightPercentage",
                table: "ExamQuestionLearningOutcomes");

            migrationBuilder.DropColumn(
                name: "WeightPercentage",
                table: "AssessmentComponentLearningOutcomes");
        }
    }
}
