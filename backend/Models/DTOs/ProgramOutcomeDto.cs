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
