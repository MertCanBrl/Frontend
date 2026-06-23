namespace Backend.Models.DTOs;

public class ApprovalListItemDto
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string? InstructorName { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string ContentStatus { get; set; } = string.Empty;
}
