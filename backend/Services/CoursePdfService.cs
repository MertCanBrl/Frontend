using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Backend.Models.DTOs;

namespace Backend.Services;

/// <summary>
/// "Ders Öğrenme Çıktıları Tanım ve Eşleştirme Formu" resmi PDF çıktısını üretir.
/// Üç bölümden oluşur: (1) Dersin Öğrenme Çıktıları, (2) ÖÇ–PÇ eşleştirme matrisi,
/// (3) Program çıktıları listesi. Üst bilgide kurum hiyerarşisi + logo/QR alanları,
/// alt bilgide imza blokları yer alır.
/// </summary>
public static class CoursePdfService
{
    // Kurum hiyerarşisi (fakülte ve üniversite sabit, bölüm derste dinamik gelir)
    private const string University = "Erciyes Üniversitesi";
    private const string Faculty = "Mühendislik Fakültesi";
    private const string FormTitle = "Ders Öğrenme Çıktıları Tanım ve Eşleştirme Formu";

    public static byte[] Generate(AdminCourseContentDto data)
    {
        var course = data.CourseDetail;
        var department = string.IsNullOrWhiteSpace(course.Department) ? "—" : course.Department;
        var programOutcomes = data.Matrix.ProgramOutcomes;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.3f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Black));

                ComposeHeader(page, course, department);
                ComposeFooter(page, course);

                page.Content().PaddingTop(12).Column(col =>
                {
                    col.Spacing(11);

                    // ── 1. Dersin Öğrenme Çıktıları ──────────────────────────
                    col.Item().Element(c => SectionTitle(c, "1. Dersin Öğrenme Çıktıları"));
                    if (data.LearningOutcomes.Count == 0)
                        col.Item().Element(c => EmptyNote(c, "Tanımlı öğrenme çıktısı bulunmamaktadır."));
                    else
                        col.Item().Element(c => ComposeLearningOutcomesTable(c, data));

                    // ── 2. ÖÇ – PÇ Eşleştirmesi (Matris) ─────────────────────
                    col.Item().Element(c => SectionTitle(c, "2. ÖÇ – PÇ Eşleştirmesi"));
                    if (data.LearningOutcomes.Count == 0 || programOutcomes.Count == 0)
                        col.Item().Element(c => EmptyNote(c, "Eşleştirme için yeterli veri bulunmamaktadır."));
                    else
                        col.Item().Element(c => ComposeMatrixTable(c, data));

                    // ── 3. Program Çıktıları ─────────────────────────────────
                    col.Item().Element(c => SectionTitle(c, "3. Program Çıktıları"));
                    if (programOutcomes.Count == 0)
                        col.Item().Element(c => EmptyNote(c, "Tanımlı program çıktısı bulunmamaktadır."));
                    else
                        col.Item().Element(c => ComposeProgramOutcomesTable(c, programOutcomes));
                });
            });
        })
        .GeneratePdf();
    }

    // ── Üst bilgi: logo / kurum hiyerarşisi / QR ────────────────────────────
    private static void ComposeHeader(PageDescriptor page, CourseDetailDto course, string department)
    {
        page.Header().Column(header =>
        {
            header.Item().Row(row =>
            {
                // Sol: logo placeholder
                row.ConstantItem(70).Height(48).Border(0.75f).BorderColor(Colors.Grey.Medium)
                    .AlignMiddle().AlignCenter().Text("LOGO").FontSize(9).FontColor(Colors.Grey.Darken1);

                // Orta: kurum hiyerarşisi + form başlığı + ders + tarih
                row.RelativeItem().PaddingHorizontal(10).Column(c =>
                {
                    c.Item().AlignCenter()
                        .Text($"{University}  ›  {Faculty}  ›  {department}")
                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().PaddingTop(3).AlignCenter()
                        .Text(FormTitle).FontSize(13).Bold();
                    c.Item().PaddingTop(4).AlignCenter()
                        .Text($"{course.Code} – {course.Name}").FontSize(10).SemiBold();
                    c.Item().PaddingTop(1).AlignCenter()
                        .Text($"Tarih: {DateTime.Now:dd/MM/yyyy}").FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                // Sağ: QR placeholder
                row.ConstantItem(48).Height(48).Border(0.75f).BorderColor(Colors.Grey.Medium)
                    .AlignMiddle().AlignCenter().Text("QR").FontSize(9).FontColor(Colors.Grey.Darken1);
            });

            header.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    // ── Alt bilgi: imza blokları + sayfa numarası ──────────────────────────
    private static void ComposeFooter(PageDescriptor page, CourseDetailDto course)
    {
        page.Footer().Column(footer =>
        {
            // İmza blokları (sayfanın altına sabitlenir — resmi form düzeni)
            footer.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text(course.InstructorName ?? "—").FontSize(9).SemiBold();
                    c.Item().PaddingTop(3).LineHorizontal(0.75f).LineColor(Colors.Grey.Darken1);
                    c.Item().PaddingTop(3).AlignCenter().Text("Sorumlu Öğretim Elemanı")
                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                });

                row.ConstantItem(50); // bloklar arası boşluk

                row.RelativeItem().Column(c =>
                {
                    c.Item().AlignCenter().Text(" ").FontSize(9); // imza için isim boşluğu
                    c.Item().PaddingTop(3).LineHorizontal(0.75f).LineColor(Colors.Grey.Darken1);
                    c.Item().PaddingTop(3).AlignCenter().Text("Bölüm Başkanı")
                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                });
            });

            footer.Item().PaddingTop(5).AlignRight().Text(x =>
            {
                x.Span("Sayfa ").FontSize(8).FontColor(Colors.Grey.Medium);
                x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                x.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }

    // ── Tablo 1: ÖÇ Kodu | Açıklama | Bileşen ──────────────────────────────
    private static void ComposeLearningOutcomesTable(IContainer container, AdminCourseContentDto data)
    {
        container.Table(tbl =>
        {
            tbl.ColumnsDefinition(c =>
            {
                c.ConstantColumn(58);   // ÖÇ Kodu
                c.RelativeColumn();     // Açıklama
                c.ConstantColumn(80);   // Bileşen
            });

            tbl.Header(h =>
            {
                h.Cell().Element(HeaderCell).Text("ÖÇ Kodu").Bold();
                h.Cell().Element(HeaderCell).Text("Açıklama").Bold();
                h.Cell().Element(HeaderCell).AlignCenter().Text("Bileşen").Bold();
            });

            foreach (var lo in data.LearningOutcomes)
            {
                tbl.Cell().Element(BodyCell).Text(lo.Code).SemiBold();
                tbl.Cell().Element(BodyCell).Text(lo.Description);
                tbl.Cell().Element(BodyCell).AlignCenter().Text(lo.Component ?? "—");
            }
        });
    }

    // ── Tablo 2: ÖÇ-PÇ katkı matrisi ───────────────────────────────────────
    private static void ComposeMatrixTable(IContainer container, AdminCourseContentDto data)
    {
        var programOutcomes = data.Matrix.ProgramOutcomes;

        container.Table(tbl =>
        {
            tbl.ColumnsDefinition(c =>
            {
                c.ConstantColumn(58);                       // ÖÇ kod sütunu
                foreach (var _ in programOutcomes)
                    c.RelativeColumn();                     // her PÇ için eşit genişlik
            });

            tbl.Header(h =>
            {
                h.Cell().Element(HeaderCell).AlignCenter().AlignMiddle()
                    .Text("ÖÇ \\ PÇ").Bold().FontSize(7);
                foreach (var po in programOutcomes)
                    h.Cell().Element(HeaderCell).AlignCenter().AlignMiddle()
                        .Text(po.Code).Bold().FontSize(7);
            });

            foreach (var lo in data.LearningOutcomes)
            {
                tbl.Cell().Element(BodyCell).AlignMiddle().Text(lo.Code).SemiBold().FontSize(8);

                foreach (var po in programOutcomes)
                {
                    var level = data.Matrix.Mappings
                        .FirstOrDefault(m => m.LearningOutcomeId == lo.Id && m.ProgramOutcomeId == po.Id)
                        ?.ContributionLevel ?? 0;

                    tbl.Cell().Element(BodyCell).AlignCenter().AlignMiddle()
                        .Text(level == 0 ? "" : level.ToString()).FontSize(8);
                }
            }
        });
    }

    // ── Tablo 3: Program çıktıları (3 sütunlu split) ───────────────────────
    private static void ComposeProgramOutcomesTable(IContainer container, List<ProgramOutcomeDto> programOutcomes)
    {
        const int groups = 3;
        var perColumn = (int)Math.Ceiling(programOutcomes.Count / (double)groups);

        container.Table(tbl =>
        {
            tbl.ColumnsDefinition(c =>
            {
                for (var g = 0; g < groups; g++)
                {
                    c.ConstantColumn(42);   // PÇ kod
                    c.RelativeColumn();     // açıklama
                }
            });

            tbl.Header(h =>
            {
                for (var g = 0; g < groups; g++)
                {
                    h.Cell().Element(HeaderCell).Text("PÇ").Bold().FontSize(8);
                    h.Cell().Element(HeaderCell).Text("Açıklama").Bold().FontSize(8);
                }
            });

            // Sütun bazlı (column-major) dağıtım: ilk sütun dolar, sonra ikinci...
            for (var r = 0; r < perColumn; r++)
            {
                for (var g = 0; g < groups; g++)
                {
                    var idx = g * perColumn + r;
                    if (idx < programOutcomes.Count)
                    {
                        tbl.Cell().Element(BodyCell).Text(programOutcomes[idx].Code).SemiBold().FontSize(8);
                        tbl.Cell().Element(BodyCell).Text(programOutcomes[idx].Description).FontSize(8);
                    }
                    else
                    {
                        tbl.Cell().Element(BodyCell).Text("");
                        tbl.Cell().Element(BodyCell).Text("");
                    }
                }
            }
        });
    }

    // ── Ortak hücre / başlık stilleri ──────────────────────────────────────
    private static IContainer HeaderCell(IContainer c) =>
        c.Border(0.5f).BorderColor(Colors.Grey.Medium)
         .Background(Colors.Grey.Lighten3)
         .PaddingVertical(4).PaddingHorizontal(5);

    private static IContainer BodyCell(IContainer c) =>
        c.Border(0.5f).BorderColor(Colors.Grey.Lighten1)
         .PaddingVertical(3).PaddingHorizontal(5);

    private static void SectionTitle(IContainer container, string title) =>
        container.PaddingBottom(2).Text(title).FontSize(11).Bold().FontColor(Colors.BlueGrey.Darken3);

    private static void EmptyNote(IContainer container, string text) =>
        container.PaddingVertical(4).PaddingHorizontal(2)
            .Text(text).FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
}
