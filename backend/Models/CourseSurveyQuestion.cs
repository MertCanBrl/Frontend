namespace Backend.Models;

public class CourseSurveyQuestion
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public string QuestionText { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<SurveyQuestionLOWeight> LOWeights { get; set; } = new List<SurveyQuestionLOWeight>();
}
