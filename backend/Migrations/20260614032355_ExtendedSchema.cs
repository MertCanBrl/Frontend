using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ExtendedSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Akts",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ClassYear",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CourseType",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "Courses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "WeeklyHours",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CourseTopics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    OrderNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseTopics_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningOutcomes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    BloomLevel = table.Column<string>(type: "text", nullable: true),
                    Component = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningOutcomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningOutcomes_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramOutcomeGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramOutcomeGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProgramOutcomes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    GroupId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramOutcomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramOutcomes_ProgramOutcomeGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "ProgramOutcomeGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LOPOMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LearningOutcomeId = table.Column<int>(type: "integer", nullable: false),
                    ProgramOutcomeId = table.Column<int>(type: "integer", nullable: false),
                    ContributionLevel = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOPOMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LOPOMappings_LearningOutcomes_LearningOutcomeId",
                        column: x => x.LearningOutcomeId,
                        principalTable: "LearningOutcomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LOPOMappings_ProgramOutcomes_ProgramOutcomeId",
                        column: x => x.ProgramOutcomeId,
                        principalTable: "ProgramOutcomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Akts", "ClassYear", "CourseType", "Department", "IsLocked", "WeeklyHours" },
                values: new object[] { 6, 1, "Teorik+Lab", "Bilgisayar Mühendisliği", false, 4 });

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Akts", "ClassYear", "CourseType", "Department", "IsLocked", "WeeklyHours" },
                values: new object[] { 5, 2, "Teorik", "Bilgisayar Mühendisliği", false, 3 });

            migrationBuilder.InsertData(
                table: "ProgramOutcomeGroups",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "GRP1", "Teknik Yeterlilikler" },
                    { 2, "GRP2", "Mesleki ve Etik Sorumluluklar" }
                });

            migrationBuilder.InsertData(
                table: "ProgramOutcomes",
                columns: new[] { "Id", "Code", "Description", "Details", "GroupId" },
                values: new object[,]
                {
                    { 1, "PÇ1", "Matematik, fen bilimleri ve bilgisayar mühendisliği konularında yeterli bilgi birikimi", null, 1 },
                    { 2, "PÇ2", "Bilgisayar mühendisliği problemlerini saptama, tanımlama, formüle etme ve çözme becerisi", null, 1 },
                    { 3, "PÇ3", "Karmaşık sistemleri, süreçleri veya ürünleri tasarlama becerisi", null, 1 },
                    { 4, "PÇ4", "Karmaşık mühendislik problemlerini araştırma becerisi", null, 1 },
                    { 5, "PÇ5", "Modern araç ve teknikleri kullanma becerisi", null, 1 },
                    { 6, "PÇ6", "Mühendislik uygulamalarının toplumsal ve küresel boyutlarını anlama", null, 2 },
                    { 7, "PÇ7", "Mesleki ve etik sorumluluk bilinci", null, 2 },
                    { 8, "PÇ8", "Etkin iletişim kurma becerisi", null, 2 },
                    { 9, "PÇ9", "Yaşam boyu öğrenmenin gerekliliği bilinci", null, 2 },
                    { 10, "PÇ10", "Proje yönetimi, risk yönetimi ve değişiklik yönetimi becerisi", null, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseTopics_CourseId",
                table: "CourseTopics",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningOutcomes_CourseId",
                table: "LearningOutcomes",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_LOPOMappings_LearningOutcomeId_ProgramOutcomeId",
                table: "LOPOMappings",
                columns: new[] { "LearningOutcomeId", "ProgramOutcomeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LOPOMappings_ProgramOutcomeId",
                table: "LOPOMappings",
                column: "ProgramOutcomeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramOutcomes_GroupId",
                table: "ProgramOutcomes",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseTopics");

            migrationBuilder.DropTable(
                name: "LOPOMappings");

            migrationBuilder.DropTable(
                name: "LearningOutcomes");

            migrationBuilder.DropTable(
                name: "ProgramOutcomes");

            migrationBuilder.DropTable(
                name: "ProgramOutcomeGroups");

            migrationBuilder.DropColumn(
                name: "Akts",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ClassYear",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CourseType",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "WeeklyHours",
                table: "Courses");
        }
    }
}
