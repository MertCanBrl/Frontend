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
}
