using System.ComponentModel.DataAnnotations;

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
    [Range(1, 999)]
    public int OrderNumber { get; set; }

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }
}
