namespace Backend.Models.DTOs;

public class AdminCourseContentDto
{
    public CourseDetailDto CourseDetail { get; set; } = new();
    public List<CourseTopicDto> Topics { get; set; } = new();
    public List<LearningOutcomeDto> LearningOutcomes { get; set; } = new();
    public MappingMatrixDto Matrix { get; set; } = new();
    public List<SurveyQuestionDto> SurveyQuestions { get; set; } = new();
}
