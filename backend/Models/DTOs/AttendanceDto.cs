namespace Backend.Models.DTOs;

// ── Devam (devamsızlık) matrisi ───────────────────────────────────────────────

public class AttendanceMatrixDto
{
    public int CourseId { get; set; }
    public int TotalWeeks { get; set; }
    public decimal LimitPercent { get; set; }
    public List<AttendanceStudentRowDto> Students { get; set; } = [];
    public AttendanceSummaryDto Summary { get; set; } = new();
}

public class AttendanceStudentRowDto
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    // Devamsız olunan hafta numaraları (1..TotalWeeks).
    public List<int> AbsentWeeks { get; set; } = [];
    public int AbsentCount { get; set; }
    public decimal AbsenceRate { get; set; }
    // Failed | Risk | Safe
    public string Status { get; set; } = string.Empty;
}

public class AttendanceSummaryDto
{
    public int TotalStudents { get; set; }
    public int FailedCount { get; set; }
    public int RiskCount { get; set; }
}

// ── Kaydetme istekleri ────────────────────────────────────────────────────────

public class SaveAttendanceRequest
{
    public List<SaveAttendanceStudentRequest> Students { get; set; } = [];
}

public class SaveAttendanceStudentRequest
{
    public int StudentId { get; set; }
    public List<int> AbsentWeeks { get; set; } = [];
}

public class SaveAttendanceSettingsRequest
{
    public int TotalWeeks { get; set; }
    public decimal LimitPercent { get; set; }
}
