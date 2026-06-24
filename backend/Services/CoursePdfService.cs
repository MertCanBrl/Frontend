using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Backend.Models.DTOs;

namespace Backend.Services;

public static class CoursePdfService
{
    public static byte[] Generate(AdminCourseContentDto data)
    {
        var course = data.CourseDetail;

        return Document.Create(container =>
        {
            // ── A4 Portre: Tüm bölümler (kapak, bilgiler, konular, ÖÇ, anket) ──
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text($"{course.Code} – {course.Name}")
                        .FontSize(15).Bold().FontColor(Colors.BlueGrey.Darken4);
                    col.Item().PaddingTop(3)
                        .Text($"Öğretim Üyesi: {course.InstructorName ?? "—"}  ·  Dönem: {course.Semester}  ·  Kredi: {course.Credit}")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Sayfa ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(18);

                    // ── Genel Bilgiler ─────────────────────────────────────
                    col.Item().Column(inner =>
                    {
                        inner.Item().Text("Ders Bilgileri")
                            .FontSize(11).Bold().FontColor(Colors.BlueGrey.Darken4);
                        inner.Item().PaddingTop(6).Table(tbl =>
                        {
                            tbl.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1); c.RelativeColumn(2);
                                c.RelativeColumn(1); c.RelativeColumn(2);
                            });

                            void Row(string label, string value, int rowIndex)
                            {
                                var bg = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;
                                tbl.Cell().Background(bg).Padding(5)
                                    .Text(label).FontSize(9).FontColor(Colors.Grey.Darken2);
                                tbl.Cell().Background(bg).Padding(5)
                                    .Text(value).FontSize(9).Bold();
                            }

                            Row("Ders Kodu", course.Code, 0);
                            Row("Ders Adı", course.Name, 0);
                            Row("Ders Tipi", course.CourseType ?? "—", 1);
                            Row("Zorunluluk", course.IsMandatory ? "Zorunlu" : "Seçmeli", 1);
                            Row("Sınıf", $"{course.ClassYear}. Sınıf", 2);
                            Row("Dönem", course.Semester, 2);
                            Row("Kredi", course.Credit.ToString(), 3);
                            Row("AKTS", course.Akts > 0 ? course.Akts.ToString() : "—", 3);
                            Row("Haftalık Saat", course.WeeklyHours > 0 ? course.WeeklyHours.ToString() : "—", 4);
                            Row("Bölüm", string.IsNullOrWhiteSpace(course.Department) ? "—" : course.Department, 4);
                        });
                    });

                    // ── Ders Tanımı ───────────────────────────────────────
                    if (!string.IsNullOrWhiteSpace(course.Description))
                    {
                        col.Item().Column(inner =>
                        {
                            inner.Item().Text("Ders Tanımı")
                                .FontSize(11).Bold().FontColor(Colors.BlueGrey.Darken4);
                            inner.Item().PaddingTop(5)
                                .Background(Colors.Grey.Lighten5).Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(8).Text(course.Description).FontSize(9).LineHeight(1.5f);
                        });
                    }

                    // ── Ders Amacı ────────────────────────────────────────
                    if (!string.IsNullOrWhiteSpace(course.Objective))
                    {
                        col.Item().Column(inner =>
                        {
                            inner.Item().Text("Ders Amacı")
                                .FontSize(11).Bold().FontColor(Colors.BlueGrey.Darken4);
                            inner.Item().PaddingTop(5)
                                .Background(Colors.Grey.Lighten5).Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(8).Text(course.Objective).FontSize(9).LineHeight(1.5f);
                        });
                    }

                    // ── Ders Konuları ─────────────────────────────────────
                    if (data.Topics.Count > 0)
                    {
                        col.Item().Column(inner =>
                        {
                            inner.Item().Text("Ders Konuları")
                                .FontSize(11).Bold().FontColor(Colors.BlueGrey.Darken4);
                            inner.Item().PaddingTop(6).Table(tbl =>
                            {
                                tbl.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(28);
                                    c.RelativeColumn(3);
                                    c.RelativeColumn(5);
                                });

                                tbl.Header(h =>
                                {
                                    h.Cell().Background(Colors.BlueGrey.Lighten5).Padding(5).AlignCenter().Text("#").FontSize(9).Bold();
                                    h.Cell().Background(Colors.BlueGrey.Lighten5).Padding(5).Text("Konu Başlığı").FontSize(9).Bold();
                                    h.Cell().Background(Colors.BlueGrey.Lighten5).Padding(5).Text("Açıklama").FontSize(9).Bold();
                                });

                                for (int i = 0; i < data.Topics.Count; i++)
                                {
                                    var t = data.Topics[i];
                                    var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;
                                    tbl.Cell().Background(bg).Padding(5).AlignCenter().Text((i + 1).ToString()).FontSize(9);
                                    tbl.Cell().Background(bg).Padding(5).Text(t.Title).FontSize(9);
                                    tbl.Cell().Background(bg).Padding(5).Text(t.Description ?? "—").FontSize(9);
                                }
                            });
                        });
                    }

                    // ── Öğrenme Çıktıları ─────────────────────────────────
                    if (data.LearningOutcomes.Count > 0)
                    {
                        col.Item().Column(inner =>
                        {
                            inner.Item().Text("Öğrenme Çıktıları (ÖÇ)")
                                .FontSize(11).Bold().FontColor(Colors.BlueGrey.Darken4);
                            inner.Item().PaddingTop(6).Table(tbl =>
                            {
                                tbl.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(42);
                                    c.RelativeColumn(6);
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(2);
                                });

                                tbl.Header(h =>
                                {
                                    h.Cell().Background(Colors.BlueGrey.Lighten5).Padding(5).Text("Kod").FontSize(9).Bold();
                                    h.Cell().Background(Colors.BlueGrey.Lighten5).Padding(5).Text("Öğrenme Çıktısı").FontSize(9).Bold();
                                    h.Cell().Background(Colors.BlueGrey.Lighten5).Padding(5).Text("Bileşen").FontSize(9).Bold();
                                    h.Cell().Background(Colors.BlueGrey.Lighten5).Padding(5).Text("Bloom Düzeyi").FontSize(9).Bold();
                                });

                                for (int i = 0; i < data.LearningOutcomes.Count; i++)
                                {
                                    var lo = data.LearningOutcomes[i];
                                    var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;
                                    tbl.Cell().Background(bg).Padding(5).Text(lo.Code).FontSize(9).Bold();
                                    tbl.Cell().Background(bg).Padding(5).Text(lo.Description).FontSize(9);
                                    tbl.Cell().Background(bg).Padding(5).Text(lo.Component ?? "—").FontSize(9);
                                    tbl.Cell().Background(bg).Padding(5).Text(lo.BloomLevel ?? "—").FontSize(9);
                                }
                            });
                        });
                    }

                    // ── Anket Soruları ────────────────────────────────────
                    if (data.SurveyQuestions.Count > 0)
                    {
                        col.Item().Column(inner =>
                        {
                            inner.Item().Text("Anket Soruları")
                                .FontSize(11).Bold().FontColor(Colors.BlueGrey.Darken4);
                            inner.Item().PaddingTop(6).Table(tbl =>
                            {
                                tbl.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(28);
                                    c.RelativeColumn(6);
                                    c.ConstantColumn(48);
                                    c.ConstantColumn(42);
                                });

                                tbl.Header(h =>
                                {
                                    h.Cell().Background(Colors.BlueGrey.Lighten5).Padding(5).AlignCenter().Text("#").FontSize(9).Bold();
                                    h.Cell().Background(Colors.BlueGrey.Lighten5).Padding(5).Text("Soru Metni").FontSize(9).Bold();
                                    h.Cell().Background(Colors.BlueGrey.Lighten5).Padding(5).Text("ÖÇ").FontSize(9).Bold();
                                    h.Cell().Background(Colors.BlueGrey.Lighten5).Padding(5).Text("Durum").FontSize(9).Bold();
                                });

                                for (int i = 0; i < data.SurveyQuestions.Count; i++)
                                {
                                    var q = data.SurveyQuestions[i];
                                    var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;
                                    tbl.Cell().Background(bg).Padding(5).AlignCenter().Text((i + 1).ToString()).FontSize(9);
                                    tbl.Cell().Background(bg).Padding(5).Text(q.QuestionText).FontSize(9);
                                    tbl.Cell().Background(bg).Padding(5).Text(q.LearningOutcomeCode ?? "—").FontSize(9);
                                    tbl.Cell().Background(bg).Padding(5).Text(q.IsActive ? "Aktif" : "Pasif").FontSize(9);
                                }
                            });
                        });
                    }
                });
            });

            // ── A4 Yatay: ÖÇ-PÇ Eşleştirme Matrisi ─────────────────────
            if (data.LearningOutcomes.Count > 0 && data.Matrix.ProgramOutcomes.Count > 0)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("ÖÇ-PÇ Eşleştirme Matrisi")
                            .FontSize(13).Bold().FontColor(Colors.BlueGrey.Darken4);
                        col.Item().PaddingTop(2)
                            .Text($"{course.Code} – {course.Name}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Footer().AlignRight().Text(x =>
                    {
                        x.Span("Sayfa ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                        x.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                    });

                    page.Content().PaddingTop(10).Table(tbl =>
                    {
                        tbl.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(70);
                            foreach (var _ in data.Matrix.ProgramOutcomes)
                                c.RelativeColumn(1);
                        });

                        tbl.Header(h =>
                        {
                            h.Cell().Background(Colors.BlueGrey.Lighten5)
                                .Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(4).AlignCenter().AlignMiddle()
                                .Text("ÖÇ \\ PÇ").FontSize(8).Bold();

                            foreach (var po in data.Matrix.ProgramOutcomes)
                            {
                                h.Cell().Background(Colors.BlueGrey.Lighten5)
                                    .Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(3).AlignCenter().AlignMiddle()
                                    .Text(po.Code).FontSize(7).Bold();
                            }
                        });

                        foreach (var lo in data.LearningOutcomes)
                        {
                            tbl.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(4).AlignMiddle()
                                .Text(lo.Code).FontSize(8).Bold();

                            foreach (var po in data.Matrix.ProgramOutcomes)
                            {
                                var level = data.Matrix.Mappings
                                    .FirstOrDefault(m =>
                                        m.LearningOutcomeId == lo.Id &&
                                        m.ProgramOutcomeId == po.Id)
                                    ?.ContributionLevel ?? 0;

                                var bg = level switch
                                {
                                    1 => Colors.Green.Lighten4,
                                    2 => Colors.Green.Lighten3,
                                    3 => Colors.Yellow.Lighten3,
                                    4 => Colors.Orange.Lighten3,
                                    5 => Colors.Orange.Darken1,
                                    _ => Colors.Grey.Lighten4
                                };

                                tbl.Cell().Background(bg)
                                    .Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                                    .AlignCenter().AlignMiddle()
                                    .Text(level == 0 ? "" : level.ToString())
                                    .FontSize(9).Bold();
                            }
                        }
                    });
                });
            }
        })
        .GeneratePdf();
    }
}
