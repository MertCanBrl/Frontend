namespace Backend.Models.DTOs;

public class AssessmentComponentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public DateTime? Date { get; set; }
    public string? Description { get; set; }
}

public class SaveAssessmentComponentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public DateTime? Date { get; set; }
    public string? Description { get; set; }
}
