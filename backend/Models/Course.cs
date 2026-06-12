namespace Backend.Models;

public class Course
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;       // örn. "BIL101"
    public string Name { get; set; } = string.Empty;       // örn. "Programlamaya Giriş"
    public string Semester { get; set; } = string.Empty;   // örn. "2024-Güz"
    public int Credit { get; set; }                        // kredi
    public bool IsMandatory { get; set; }                  // zorunlu mu, seçmeli mi

    // Dersi veren öğretmen (navigation property)
    public int? InstructorId { get; set; }
    public User? Instructor { get; set; }

    // Bu derse kayıtlı öğrenciler (navigation property)
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}