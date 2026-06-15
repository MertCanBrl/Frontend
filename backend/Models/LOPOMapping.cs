namespace Backend.Models;

public class LOPOMapping
{
    public int Id { get; set; }

    public int LearningOutcomeId { get; set; }
    public LearningOutcome LearningOutcome { get; set; } = null!;

    public int ProgramOutcomeId { get; set; }
    public ProgramOutcome ProgramOutcome { get; set; } = null!;

    public int ContributionLevel { get; set; } = 0;    // 0-5
}
