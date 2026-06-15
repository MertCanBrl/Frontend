namespace Backend.Models.DTOs;

public class LearningOutcomeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? BloomLevel { get; set; }
    public string? Component { get; set; }
}

public class SaveLearningOutcomeRequest
{
    public string Description { get; set; } = string.Empty;
    public string? BloomLevel { get; set; }
    public string? Component { get; set; }
}
