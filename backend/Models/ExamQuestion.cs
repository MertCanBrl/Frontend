namespace Backend.Models;

public class ExamQuestion
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public Exam Exam { get; set; } = null!;
    public int QuestionNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string Difficulty { get; set; } = string.Empty;  // Kolay, Orta, Zor
    public int? BookletAQuestionNumber { get; set; }
    public int? BookletBQuestionNumber { get; set; }
    public int? BookletCQuestionNumber { get; set; }
    public int? BookletDQuestionNumber { get; set; }
    public List<ExamQuestionLearningOutcome> LearningOutcomeMappings { get; set; } = [];
}

public class ExamQuestionLearningOutcome
{
    public int ExamQuestionId { get; set; }
    public ExamQuestion ExamQuestion { get; set; } = null!;
    public int LearningOutcomeId { get; set; }
    public LearningOutcome LearningOutcome { get; set; } = null!;
}
