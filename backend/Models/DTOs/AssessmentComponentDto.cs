namespace Backend.Models.DTOs;

public class AssessmentComponentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public DateTime? Date { get; set; }
    public string? Description { get; set; }
    public decimal MaxScore { get; set; }
    public bool IsIncludedInAverage { get; set; }
    public string? GradeGroup { get; set; }
    public decimal GroupWeightPercentage { get; set; }
    public List<LearningOutcomeWeightDto> LearningOutcomeWeights { get; set; } = [];
}

public class SaveAssessmentComponentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public DateTime? Date { get; set; }
    public string? Description { get; set; }
    public decimal MaxScore { get; set; } = 100;
    public bool IsIncludedInAverage { get; set; } = false;
    public string? GradeGroup { get; set; }
    public decimal GroupWeightPercentage { get; set; } = 0;
    public List<SaveLearningOutcomeWeightRequest> LearningOutcomeWeights { get; set; } = [];
}

// ── Component Grade Entry ─────────────────────────────────────────────────────

public class ComponentGradeEntryDto
{
    public int ComponentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public List<ComponentStudentGradeDto> Students { get; set; } = [];
}

public class ComponentStudentGradeDto
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public decimal? Score { get; set; }
}

public class SaveComponentGradeEntryRequest
{
    public List<SaveComponentStudentGradeRequest> Students { get; set; } = [];
}

public class SaveComponentStudentGradeRequest
{
    public int StudentId { get; set; }
    public decimal? Score { get; set; }
}
