namespace Backend.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;   // "Admin" veya "Instructor"

    // Bu öğretmene atanan dersler (navigation property)
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}