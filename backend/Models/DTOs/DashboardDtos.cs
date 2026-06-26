namespace Backend.Models.DTOs;

public class InstructorDashboardDto
{
    public string? LatestSemester { get; set; }
    public List<CourseSummaryReportDto> CourseSummaries { get; set; } = [];
    public List<PoContributionDto> PoContribution { get; set; } = [];
    public List<AverageTrendDto> AverageTrend { get; set; } = [];
}

public class CourseSummaryReportDto
{
    public int CourseId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Semester { get; set; } = "";
    public int TotalStudents { get; set; }
    public double? ClassAverage { get; set; }
    public double? PassRate { get; set; }
}

public class PoContributionDto
{
    public string PoCode { get; set; } = "";
    public string PoDescription { get; set; } = "";
    public int[] CountsByWeight { get; set; } = new int[5];
}

public class AverageTrendDto
{
    public string Semester { get; set; } = "";
    public string CourseCode { get; set; } = "";
    public string CourseName { get; set; } = "";
    public double? Average { get; set; }
}
