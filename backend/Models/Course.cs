namespace Backend.Models;

public class Course
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public int Credit { get; set; }
    public bool IsMandatory { get; set; }

    // Yeni alanlar
    public int Akts { get; set; } = 0;
    public int WeeklyHours { get; set; } = 3;
    public string Department { get; set; } = string.Empty;
    public int ClassYear { get; set; } = 1;
    public string CourseType { get; set; } = "Teorik";
    public bool IsLocked { get; set; } = false;

    public int? InstructorId { get; set; }
    public User? Instructor { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<CourseTopic> CourseTopics { get; set; } = new List<CourseTopic>();
    public ICollection<LearningOutcome> LearningOutcomes { get; set; } = new List<LearningOutcome>();
}
