using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class Course
{
    public int Id { get; set; }

    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Semester { get; set; } = string.Empty;

    public int Credit { get; set; }
    public bool IsMandatory { get; set; }

    public int Akts { get; set; } = 0;
    public int WeeklyHours { get; set; } = 3;

    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    public int ClassYear { get; set; } = 1;

    [MaxLength(50)]
    public string CourseType { get; set; } = "Teorik";

    public bool IsLocked { get; set; } = false;

    [MaxLength(4000)]
    public string? Description { get; set; }

    [MaxLength(4000)]
    public string? Objective { get; set; }

    [MaxLength(30)]
    public string ContentStatus { get; set; } = "Draft";

    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    [MaxLength(2000)]
    public string? ReviewNote { get; set; }

    public int? ReviewedByUserId { get; set; }
    public User? ReviewedBy { get; set; }

    public int? InstructorId { get; set; }
    public User? Instructor { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<CourseTopic> CourseTopics { get; set; } = new List<CourseTopic>();
    public ICollection<LearningOutcome> LearningOutcomes { get; set; } = new List<LearningOutcome>();
    public ICollection<CourseSurveyQuestion> SurveyQuestions { get; set; } = new List<CourseSurveyQuestion>();
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
    public ICollection<AssessmentComponent> AssessmentComponents { get; set; } = new List<AssessmentComponent>();
}
