namespace Backend.Models.DTOs;

public class SurveyQuestionDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int? LearningOutcomeId { get; set; }
    public string? LearningOutcomeCode { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class SaveSurveyQuestionRequest
{
    public int? LearningOutcomeId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
