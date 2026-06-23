using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

public class RequestRevisionRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Revizyon notu boş olamaz.")]
    public string Note { get; set; } = string.Empty;
}
