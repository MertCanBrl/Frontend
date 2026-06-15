namespace Backend.Models;

public class ProgramOutcomeGroup
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<ProgramOutcome> ProgramOutcomes { get; set; } = new List<ProgramOutcome>();
}
