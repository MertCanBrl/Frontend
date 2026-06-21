namespace Backend.Models.DTOs;

public class StudentCourseResultDto
{
    public int StudentId { get; set; }
    public string StudentNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public decimal? Midterm { get; set; }
    public decimal? Final { get; set; }
    public decimal? MakeUp { get; set; }
}

public class RiskAnalysisDto
{
    public int StudentId { get; set; }
    public string StudentNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public decimal? GradeAverage { get; set; }
    public string RiskLevel { get; set; } = "Düşük";   // Düşük, Orta, Yüksek
    public string Suggestion { get; set; } = string.Empty;
}

public class UpdateCourseContentRequest
{
    public string? Description { get; set; }
    public string? Objective { get; set; }
}
