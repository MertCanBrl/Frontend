using System.ComponentModel.DataAnnotations;

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
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? BloomLevel { get; set; }

    [MaxLength(100)]
    public string? Component { get; set; }
}
