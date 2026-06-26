using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class GeneralSurveyQuestion
{
    public int Id { get; set; }

    [MaxLength(500)]
    public string QuestionText { get; set; } = string.Empty;

    public int OrderNumber { get; set; }
    public bool IsActive { get; set; } = true;
}
