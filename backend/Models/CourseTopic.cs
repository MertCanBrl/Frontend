namespace Backend.Models;

public class CourseTopic
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int OrderNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}
