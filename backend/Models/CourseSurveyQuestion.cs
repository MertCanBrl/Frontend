namespace Backend.Models;

public class CourseSurveyQuestion
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int? LearningOutcomeId { get; set; }
    public LearningOutcome? LearningOutcome { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
