namespace Backend.Models.DTOs;

public class InstructorCourseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public int Credit { get; set; }
    public int Akts { get; set; }
    public bool IsMandatory { get; set; }
    public string Department { get; set; } = string.Empty;
    public int ClassYear { get; set; }
    public string CourseType { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public int TopicCount { get; set; }
    public int LearningOutcomeCount { get; set; }
}

public class CourseDetailDto : InstructorCourseDto
{
    public int WeeklyHours { get; set; }
    public string? InstructorName { get; set; }
    public string? Description { get; set; }
    public string? Objective { get; set; }
}

public class UpdateCourseRequest
{
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int ClassYear { get; set; }
    public string CourseType { get; set; } = string.Empty;
    public int Credit { get; set; }
    public int Akts { get; set; }
    public int WeeklyHours { get; set; }
    public bool IsMandatory { get; set; }
}
