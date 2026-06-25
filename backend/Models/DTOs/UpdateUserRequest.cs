using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "Ad Soyad zorunludur.")]
    [MaxLength(200, ErrorMessage = "Ad Soyad en fazla 200 karakter olabilir.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [MaxLength(256, ErrorMessage = "E-posta en fazla 256 karakter olabilir.")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [MaxLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir.")]
    public string? PhoneNumber { get; set; }

    /// <summary>İşaretliyse personele "Admin" rolü, aksi halde "Instructor" rolü atanır.</summary>
    public bool IsAdmin { get; set; }
}
