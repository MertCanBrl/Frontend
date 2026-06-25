namespace Backend.Models.DTOs;

public class ExamDto
{
    public int Id { get; set; }
    public string ExamType { get; set; } = string.Empty;
    public string ExamMethod { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public int? QuestionCount { get; set; }
    public string? Description { get; set; }
    public decimal TotalScore { get; set; }
    public decimal? WeightPercentage { get; set; }
    public bool HasGrades { get; set; }
    public int GradedStudentCount { get; set; }
    public int TotalStudentCount { get; set; }
}

public class ExamDetailDto
{
    public int Id { get; set; }
    public string ExamType { get; set; } = string.Empty;
    public string ExamMethod { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public int? QuestionCount { get; set; }
    public string? Description { get; set; }
    public decimal TotalScore { get; set; }
    public decimal? WeightPercentage { get; set; }
    public List<ExamQuestionDto> Questions { get; set; } = [];
}

public class ExamQuestionDto
{
    public int Id { get; set; }
    public int QuestionNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public int? BookletAQuestionNumber { get; set; }
    public int? BookletBQuestionNumber { get; set; }
    public int? BookletCQuestionNumber { get; set; }
    public int? BookletDQuestionNumber { get; set; }
    public List<int> LearningOutcomeIds { get; set; } = [];
}

public class SaveExamRequest
{
    public string ExamType { get; set; } = string.Empty;
    public string ExamMethod { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public int? QuestionCount { get; set; }
    public string? Description { get; set; }
    public decimal? WeightPercentage { get; set; }
    public List<SaveExamQuestionRequest>? Questions { get; set; }
}

public class SaveExamQuestionRequest
{
    public int QuestionNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public int? BookletAQuestionNumber { get; set; }
    public int? BookletBQuestionNumber { get; set; }
    public int? BookletCQuestionNumber { get; set; }
    public int? BookletDQuestionNumber { get; set; }
    public List<int> LearningOutcomeIds { get; set; } = [];
}

// ── Grade Entry ───────────────────────────────────────────────────────────────

public class ExamGradeEntryDto
{
    public int ExamId { get; set; }
    public int CourseId { get; set; }
    public string ExamType { get; set; } = string.Empty;
    public string ExamMethod { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string? Description { get; set; }
    public List<GradeEntryQuestionDto> Questions { get; set; } = [];
    public List<GradeEntryStudentDto> Students { get; set; } = [];
}

public class GradeEntryQuestionDto
{
    public int Id { get; set; }
    public int QuestionNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
}

public class GradeEntryStudentDto
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public decimal TotalScore { get; set; }
    public bool IsCompleted { get; set; }
    public List<GradeEntryQuestionScoreDto> QuestionScores { get; set; } = [];
}

public class GradeEntryQuestionScoreDto
{
    public int QuestionId { get; set; }
    public decimal? Score { get; set; }
}

public class SaveExamGradeEntryRequest
{
    public List<SaveStudentGradeRequest> Students { get; set; } = [];
}

public class SaveStudentGradeRequest
{
    public int StudentId { get; set; }
    public List<SaveQuestionScoreRequest> QuestionScores { get; set; } = [];
}

public class SaveQuestionScoreRequest
{
    public int QuestionId { get; set; }
    public decimal Score { get; set; }
}
