namespace Backend.Models.DTOs;

public class ExamDto
{
    public int Id { get; set; }
    public string ExamType { get; set; } = string.Empty;
    public string ExamMethod { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public int? QuestionCount { get; set; }
    public string? Description { get; set; }
}

public class SaveExamRequest
{
    public string ExamType { get; set; } = string.Empty;
    public string ExamMethod { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public int? QuestionCount { get; set; }
    public string? Description { get; set; }
}
