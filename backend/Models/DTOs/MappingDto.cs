namespace Backend.Models.DTOs;

public class MappingCellDto
{
    public int LearningOutcomeId { get; set; }
    public int ProgramOutcomeId { get; set; }
    public int ContributionLevel { get; set; }
}

public class MappingMatrixDto
{
    public List<LearningOutcomeDto> LearningOutcomes { get; set; } = new();
    public List<ProgramOutcomeDto> ProgramOutcomes { get; set; } = new();
    public List<MappingCellDto> Mappings { get; set; } = new();
}
