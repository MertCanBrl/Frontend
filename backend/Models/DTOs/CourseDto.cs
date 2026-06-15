namespace Backend.Models.DTOs;

public class CourseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public int Credit { get; set; }
    public bool IsMandatory { get; set; }
    public int? InstructorId { get; set; }
    public string? InstructorName { get; set; }
}
