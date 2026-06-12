namespace Backend.Models;

public class Enrollment
{
    public int Id { get; set; }

    // Hangi öğrenci
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    // Hangi ders
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    // Notlar (henüz girilmediyse null olabilir)
    public decimal? Midterm { get; set; }   // Vize
    public decimal? Final { get; set; }      // Final
    public decimal? MakeUp { get; set; }     // Bütünleme
}