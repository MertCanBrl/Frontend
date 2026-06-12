namespace Backend.Models;

public class Student
{
    public int Id { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public bool IsActive { get; set; } = true;


    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}