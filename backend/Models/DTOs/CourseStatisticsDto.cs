namespace Backend.Models.DTOs;

public class CourseStatisticsDto
{
    // Kart 1 — Ders Genel
    public int TotalStudents { get; set; }
    public int GradedStudents { get; set; }       // başarı notu hesaplanabilen
    public decimal? ClassAverage { get; set; }    // başarı notlarının ortalaması
    public int PassCount { get; set; }            // başarı notu >= 50
    public int FailCount { get; set; }            // başarı notu < 50

    // Kart 4 — Başarı Dağılımı (kovalar)
    public List<GradeBucketDto> Distribution { get; set; } = new();
}

public class GradeBucketDto
{
    public string Label { get; set; } = "";   // örn. "50-59"
    public int Count { get; set; }            // bu aralıktaki öğrenci sayısı
}