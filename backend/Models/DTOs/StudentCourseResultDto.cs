namespace Backend.Models.DTOs;

public class StudentComponentScoreDto
{
    public int ComponentId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public decimal MaxScore { get; set; }
}

public class StudentCourseResultDto
{
    public int StudentId { get; set; }
    public string StudentNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public decimal? Midterm { get; set; }
    public decimal? Final { get; set; }
    public decimal? MakeUp { get; set; }
    public decimal? WeightedAverage { get; set; }
    public List<StudentComponentScoreDto> ComponentScores { get; set; } = [];
    public bool HasMissingGrades { get; set; }
    public List<string> MissingComponentNames { get; set; } = [];
}

public class RiskAnalysisDto
{
    public int StudentId { get; set; }
    public string StudentNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public decimal? GradeAverage { get; set; }
    public string RiskLevel { get; set; } = "Düşük";   // Düşük, Orta, Yüksek
    public string Suggestion { get; set; } = string.Empty;
    public bool HasMissingGrades { get; set; }
}

public class UpdateCourseContentRequest
{
    public string? Description { get; set; }
    public string? Objective { get; set; }
}

public class LearningOutcomeStatusDto
{
    public int LearningOutcomeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? AverageSuccess { get; set; }
    public int SourceCount { get; set; }
    public List<LoSourceDto> Sources { get; set; } = [];
}

public class LoSourceDto
{
    public string SourceType { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public decimal MaxRawScore { get; set; }
    public decimal? AverageRawScore { get; set; }
    public decimal? AverageNormalized { get; set; }
    public decimal LoWeightPercentage { get; set; } = 100m;
    public int StudentCount { get; set; }
}

public class ComponentReportItemDto
{
    public int ComponentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? GradeGroup { get; set; }
    public decimal GroupWeightPercentage { get; set; }
    public decimal MaxScore { get; set; }
    public bool IsIncludedInAverage { get; set; }
    public List<LearningOutcomeWeightDto> LearningOutcomeWeights { get; set; } = [];
    public int TotalStudents { get; set; }
    public int GradedCount { get; set; }
    public decimal? AverageScore { get; set; }
    public decimal? AverageNormalized { get; set; }
}
