namespace Backend.Models;

public class AssessmentComponent
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Weight { get; set; }           // Genel ağırlık % (eski alan, geriye dönük uyumluluk)
    public DateTime? Date { get; set; }
    public string? Description { get; set; }

    // Yeni alanlar
    public decimal MaxScore { get; set; } = 100;
    public bool IsIncludedInAverage { get; set; } = false;
    public string? GradeGroup { get; set; }        // Midterm | Final | Makeup
    public decimal GroupWeightPercentage { get; set; } = 0;   // Grup içindeki ağırlık %

    public List<AssessmentComponentLearningOutcome> LearningOutcomeMappings { get; set; } = [];
    public List<AssessmentComponentStudentGrade> StudentGrades { get; set; } = [];
}
