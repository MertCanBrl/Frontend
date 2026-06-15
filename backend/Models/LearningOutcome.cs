namespace Backend.Models;

public class LearningOutcome
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public string Code { get; set; } = string.Empty;       // ÖÇ1, ÖÇ2, ...
    public string Description { get; set; } = string.Empty;
    public string? BloomLevel { get; set; }
    public string? Component { get; set; }

    public ICollection<LOPOMapping> LOPOMappings { get; set; } = new List<LOPOMapping>();
}
