using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

public class CreateUserRequest
{
    [MaxLength(50)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [MaxLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir.")]
    public string? PhoneNumber { get; set; }

    /// <summary>İşaretliyse personele "Admin" rolü, aksi halde "Instructor" rolü atanır.</summary>
    public bool IsAdmin { get; set; }
}
