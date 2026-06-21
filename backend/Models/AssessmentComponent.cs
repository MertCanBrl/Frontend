namespace Backend.Models;

public class AssessmentComponent
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public DateTime? Date { get; set; }
    public string? Description { get; set; }
}
