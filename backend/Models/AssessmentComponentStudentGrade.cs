namespace Backend.Models;

public class AssessmentComponentStudentGrade
{
    public int Id { get; set; }
    public int AssessmentComponentId { get; set; }
    public AssessmentComponent AssessmentComponent { get; set; } = null!;
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public decimal Score { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
