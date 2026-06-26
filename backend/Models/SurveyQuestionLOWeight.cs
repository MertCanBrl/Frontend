namespace Backend.Models;

public class SurveyQuestionLOWeight
{
    public int Id { get; set; }
    public int SurveyQuestionId { get; set; }
    public CourseSurveyQuestion SurveyQuestion { get; set; } = null!;
    public int LearningOutcomeId { get; set; }
    public LearningOutcome LearningOutcome { get; set; } = null!;
    public int WeightPercentage { get; set; }
}
