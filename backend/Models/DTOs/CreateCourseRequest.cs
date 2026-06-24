using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

public class CreateCourseRequest
{
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Semester { get; set; } = string.Empty;

    [Range(1, 10)]
    public int Credit { get; set; }

    public bool IsMandatory { get; set; }

    [Range(1, 4)]
    public int ClassYear { get; set; }

    public int? InstructorId { get; set; }
}
