namespace Backend.Models;

public class ExamStudentGrade
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public Exam Exam { get; set; } = null!;
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public decimal TotalScore { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ExamQuestionStudentScore> QuestionScores { get; set; } = [];
}

public class ExamQuestionStudentScore
{
    public int Id { get; set; }
    public int ExamStudentGradeId { get; set; }
    public ExamStudentGrade ExamStudentGrade { get; set; } = null!;
    public int ExamQuestionId { get; set; }
    public ExamQuestion ExamQuestion { get; set; } = null!;
    public decimal Score { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
