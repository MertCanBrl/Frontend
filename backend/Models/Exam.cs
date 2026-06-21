namespace Backend.Models;

public class Exam
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public string ExamType { get; set; } = string.Empty;    // Vize, Final, Bütünleme, Quiz
    public string ExamMethod { get; set; } = string.Empty;  // Klasik, Test, Karma
    public DateTime? Date { get; set; }
    public int? QuestionCount { get; set; }
    public string? Description { get; set; }
}
