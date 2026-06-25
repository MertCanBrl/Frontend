namespace Backend.Models;

public class AssessmentComponentLearningOutcome
{
    public int AssessmentComponentId { get; set; }
    public AssessmentComponent AssessmentComponent { get; set; } = null!;
    public int LearningOutcomeId { get; set; }
    public LearningOutcome LearningOutcome { get; set; } = null!;
}
