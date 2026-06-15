namespace Backend.Models;

public class ProgramOutcome
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Details { get; set; }

    public int? GroupId { get; set; }
    public ProgramOutcomeGroup? Group { get; set; }

    public ICollection<LOPOMapping> LOPOMappings { get; set; } = new List<LOPOMapping>();
}
