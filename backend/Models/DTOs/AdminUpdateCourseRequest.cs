using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

public class AdminUpdateCourseRequest
{
    [Required(ErrorMessage = "Ders kodu zorunludur.")]
    [MaxLength(20, ErrorMessage = "Ders kodu en fazla 20 karakter olabilir.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ders adı zorunludur.")]
    [MaxLength(200, ErrorMessage = "Ders adı en fazla 200 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Dönem bilgisi zorunludur.")]
    [MaxLength(20, ErrorMessage = "Dönem bilgisi en fazla 20 karakter olabilir.")]
    public string Semester { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "Kredi 1 ile 10 arasında olmalıdır.")]
    public int Credit { get; set; }

    public bool IsMandatory { get; set; }

    [Range(1, 4, ErrorMessage = "Sınıf 1 ile 4 arasında olmalıdır.")]
    public int ClassYear { get; set; }

    public int? InstructorId { get; set; }
}
