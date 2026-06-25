namespace Backend.Models;

// Bir öğrencinin bir derste BELİRLİ BİR HAFTA devamsız olduğunu temsil eder.
// Kaydın varlığı = o hafta devamsız (gelmedi). Kayıt yoksa = geldi.
// Sparse saklama; AssessmentComponentStudentGrade desenini izler.
public class AttendanceRecord
{
    public int Id { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int WeekNumber { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
