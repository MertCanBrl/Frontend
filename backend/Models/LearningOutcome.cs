using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class LearningOutcome
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? BloomLevel { get; set; }

    [MaxLength(100)]
    public string? Component { get; set; }

    public ICollection<LOPOMapping> LOPOMappings { get; set; } = new List<LOPOMapping>();
    public List<ExamQuestionLearningOutcome> ExamQuestionMappings { get; set; } = [];
}
