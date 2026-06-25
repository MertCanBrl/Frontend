using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

public class ProgramOutcomeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Details { get; set; }
    public int? GroupId { get; set; }
    public string? GroupName { get; set; }
}

public class ProgramOutcomeGroupDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<ProgramOutcomeDto> ProgramOutcomes { get; set; } = new();
}

public class SaveProgramOutcomeRequest
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Details { get; set; }
    public int? GroupId { get; set; }
}

public class UpdateProgramOutcomeRequest
{
    [Required(ErrorMessage = "Kod alanı zorunludur.")]
    [MaxLength(20, ErrorMessage = "Kod en fazla 20 karakter olabilir.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama alanı zorunludur.")]
    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
    public string Description { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Detay en fazla 2000 karakter olabilir.")]
    public string? Details { get; set; }

    public int? GroupId { get; set; }
}
