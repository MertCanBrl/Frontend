namespace Backend.Models.DTOs;

public class CourseTopicDto
{
    public int Id { get; set; }
    public int OrderNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class SaveCourseTopicRequest
{
    public int OrderNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}
