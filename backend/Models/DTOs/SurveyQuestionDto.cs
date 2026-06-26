namespace Backend.Models.DTOs;

public class SurveyQuestionLOWeightDto
{
    public int LearningOutcomeId { get; set; }
    public string LearningOutcomeCode { get; set; } = string.Empty;
    public int WeightPercentage { get; set; }
}

public class SurveyQuestionDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<SurveyQuestionLOWeightDto> LOWeights { get; set; } = new();
}

public class SaveSurveyQuestionRequest
{
    public string QuestionText { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<SaveSurveyQuestionLOWeightRequest> LOWeights { get; set; } = new();
}

public class SaveSurveyQuestionLOWeightRequest
{
    public int LearningOutcomeId { get; set; }
    public int WeightPercentage { get; set; }
}

public class GeneralSurveyQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int OrderNumber { get; set; }
}

public class SaveGeneralSurveyQuestionRequest
{
    public string QuestionText { get; set; } = string.Empty;
}
