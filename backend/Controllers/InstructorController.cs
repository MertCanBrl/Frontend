using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InstructorController : ControllerBase
{
    private readonly AppDbContext _context;

    public InstructorController(AppDbContext context) => _context = context;

    private int GetUserId()
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null)
            throw new InvalidOperationException("UserId claim bulunamadı.");
        return int.Parse(claim.Value);
    }

    private async Task<bool> OwnsCourse(int courseId) =>
        await _context.Courses.AnyAsync(c => c.Id == courseId && c.InstructorId == GetUserId());

    // ── Mevcut endpointler (geriye dönük uyumluluk) ──────────────────────────

    [HttpGet("my-courses")]
    public async Task<ActionResult<IEnumerable<InstructorCourseDto>>> GetMyCourses()
    {
        var userId = GetUserId();
        var courses = await _context.Courses
            .Where(c => c.InstructorId == userId)
            .OrderBy(c => c.Code)
            .Select(c => new InstructorCourseDto
            {
                Id = c.Id, Code = c.Code, Name = c.Name, Semester = c.Semester,
                Credit = c.Credit, Akts = c.Akts, IsMandatory = c.IsMandatory,
                Department = c.Department, ClassYear = c.ClassYear,
                CourseType = c.CourseType, IsLocked = c.IsLocked,
                TopicCount = c.CourseTopics.Count,
                LearningOutcomeCount = c.LearningOutcomes.Count
            })
            .ToListAsync();
        return Ok(courses);
    }

    [HttpGet("courses/{id}")]
    public async Task<ActionResult<CourseDetailDto>> GetCourseDetail(int id)
    {
        var userId = GetUserId();
        var course = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.CourseTopics)
            .Include(c => c.LearningOutcomes)
            .FirstOrDefaultAsync(c => c.Id == id && c.InstructorId == userId);
        if (course == null) return NotFound();
        return Ok(MapToCourseDetailDto(course));
    }

    [HttpPut("courses/{id}")]
    public async Task<IActionResult> UpdateCourse(int id, UpdateCourseRequest request)
    {
        var userId = GetUserId();
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id && c.InstructorId == userId);
        if (course == null) return NotFound();
        if (course.IsLocked) return Forbid();

        course.Name = request.Name;
        course.Department = request.Department;
        course.ClassYear = request.ClassYear;
        course.CourseType = request.CourseType;
        course.Credit = request.Credit;
        course.Akts = request.Akts;
        course.WeeklyHours = request.WeeklyHours;
        course.IsMandatory = request.IsMandatory;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ── Ders İçerikleri ──────────────────────────────────────────────────────

    [HttpGet("course-contents")]
    public async Task<ActionResult<IEnumerable<InstructorCourseDto>>> GetCourseContents()
    {
        var userId = GetUserId();
        var courses = await _context.Courses
            .Where(c => c.InstructorId == userId)
            .OrderBy(c => c.Code)
            .Select(c => new InstructorCourseDto
            {
                Id = c.Id, Code = c.Code, Name = c.Name, Semester = c.Semester,
                Credit = c.Credit, Akts = c.Akts, IsMandatory = c.IsMandatory,
                Department = c.Department, ClassYear = c.ClassYear,
                CourseType = c.CourseType, IsLocked = c.IsLocked,
                TopicCount = c.CourseTopics.Count,
                LearningOutcomeCount = c.LearningOutcomes.Count
            })
            .ToListAsync();
        return Ok(courses);
    }

    [HttpGet("course-contents/{courseId:int}")]
    public async Task<ActionResult<CourseDetailDto>> GetCourseContentDetail(int courseId)
    {
        var userId = GetUserId();
        var course = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.CourseTopics)
            .Include(c => c.LearningOutcomes)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == userId);
        if (course == null) return NotFound();
        return Ok(MapToCourseDetailDto(course));
    }

    [HttpPut("course-contents/{courseId:int}/general-info")]
    public async Task<IActionResult> UpdateCourseContentInfo(int courseId, UpdateCourseContentRequest request)
    {
        var userId = GetUserId();
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == userId);
        if (course == null) return NotFound();

        course.Description = request.Description;
        course.Objective = request.Objective;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Survey Questions

    [HttpGet("course-contents/{courseId:int}/survey-questions")]
    public async Task<ActionResult<IEnumerable<SurveyQuestionDto>>> GetSurveyQuestions(int courseId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var questions = await _context.CourseSurveyQuestions
            .Include(q => q.LearningOutcome)
            .Where(q => q.CourseId == courseId)
            .Select(q => new SurveyQuestionDto
            {
                Id = q.Id,
                CourseId = q.CourseId,
                LearningOutcomeId = q.LearningOutcomeId,
                LearningOutcomeCode = q.LearningOutcome != null ? q.LearningOutcome.Code : null,
                QuestionText = q.QuestionText,
                IsActive = q.IsActive
            })
            .ToListAsync();
        return Ok(questions);
    }

    [HttpPost("course-contents/{courseId:int}/survey-questions")]
    public async Task<ActionResult<SurveyQuestionDto>> AddSurveyQuestion(int courseId, SaveSurveyQuestionRequest request)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var question = new CourseSurveyQuestion
        {
            CourseId = courseId,
            LearningOutcomeId = request.LearningOutcomeId,
            QuestionText = request.QuestionText,
            IsActive = request.IsActive
        };
        _context.CourseSurveyQuestions.Add(question);
        await _context.SaveChangesAsync();

        string? loCode = null;
        if (question.LearningOutcomeId.HasValue)
        {
            var lo = await _context.LearningOutcomes.FindAsync(question.LearningOutcomeId.Value);
            loCode = lo?.Code;
        }

        return Ok(new SurveyQuestionDto
        {
            Id = question.Id, CourseId = question.CourseId,
            LearningOutcomeId = question.LearningOutcomeId, LearningOutcomeCode = loCode,
            QuestionText = question.QuestionText, IsActive = question.IsActive
        });
    }

    [HttpPut("course-contents/{courseId:int}/survey-questions/{questionId:int}")]
    public async Task<IActionResult> UpdateSurveyQuestion(int courseId, int questionId, SaveSurveyQuestionRequest request)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var question = await _context.CourseSurveyQuestions
            .FirstOrDefaultAsync(q => q.Id == questionId && q.CourseId == courseId);
        if (question == null) return NotFound();

        question.LearningOutcomeId = request.LearningOutcomeId;
        question.QuestionText = request.QuestionText;
        question.IsActive = request.IsActive;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("course-contents/{courseId:int}/survey-questions/{questionId:int}")]
    public async Task<IActionResult> DeleteSurveyQuestion(int courseId, int questionId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var question = await _context.CourseSurveyQuestions
            .FirstOrDefaultAsync(q => q.Id == questionId && q.CourseId == courseId);
        if (question == null) return NotFound();

        _context.CourseSurveyQuestions.Remove(question);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ── Dönemdeki Dersler ────────────────────────────────────────────────────

    [HttpGet("term-courses")]
    public async Task<ActionResult<IEnumerable<InstructorCourseDto>>> GetTermCourses([FromQuery] string? semester)
    {
        var userId = GetUserId();
        var query = _context.Courses.Where(c => c.InstructorId == userId);
        if (!string.IsNullOrWhiteSpace(semester))
            query = query.Where(c => c.Semester == semester);

        var courses = await query
            .OrderBy(c => c.Code)
            .Select(c => new InstructorCourseDto
            {
                Id = c.Id, Code = c.Code, Name = c.Name, Semester = c.Semester,
                Credit = c.Credit, Akts = c.Akts, IsMandatory = c.IsMandatory,
                Department = c.Department, ClassYear = c.ClassYear,
                CourseType = c.CourseType, IsLocked = c.IsLocked,
                TopicCount = c.CourseTopics.Count,
                LearningOutcomeCount = c.LearningOutcomes.Count
            })
            .ToListAsync();
        return Ok(courses);
    }

    [HttpGet("term-courses/{courseId:int}")]
    public async Task<ActionResult<CourseDetailDto>> GetTermCourseDetail(int courseId)
    {
        var userId = GetUserId();
        var course = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.CourseTopics)
            .Include(c => c.LearningOutcomes)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == userId);
        if (course == null) return NotFound();
        return Ok(MapToCourseDetailDto(course));
    }

    [HttpGet("semesters")]
    public async Task<ActionResult<IEnumerable<string>>> GetSemesters()
    {
        var userId = GetUserId();
        var semesters = await _context.Courses
            .Where(c => c.InstructorId == userId)
            .Select(c => c.Semester)
            .Distinct()
            .OrderByDescending(s => s)
            .ToListAsync();
        return Ok(semesters);
    }

    // Students

    // Students

    [HttpGet("term-courses/{courseId:int}/students")]
    public async Task<ActionResult<IEnumerable<StudentCourseResultDto>>> GetStudents(int courseId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var students = await _context.Enrollments
            .Include(e => e.Student)
            .Where(e => e.CourseId == courseId)
            .Select(e => new StudentCourseResultDto
            {
                StudentId = e.StudentId,
                StudentNo = e.Student.StudentNumber,
                FullName = e.Student.FirstName + " " + e.Student.LastName,
                Email = e.Student.Email,
                Midterm = e.Midterm,
                Final = e.Final,
                MakeUp = e.MakeUp
            })
            .OrderBy(s => s.StudentNo)
            .ToListAsync();
        return Ok(students);
    }

    [HttpPut("term-courses/{courseId:int}/students/{studentId:int}/grades")]
    public async Task<IActionResult> SaveStudentGrades(int courseId, int studentId, SaveGradesRequest request)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        if (!IsValidGrade(request.Midterm) ||
            !IsValidGrade(request.Final) ||
            !IsValidGrade(request.MakeUp))
        {
            return BadRequest("Notlar 0 ile 100 arasında olmalıdır.");
        }

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);
        if (enrollment == null) return NotFound();

        enrollment.Midterm = request.Midterm;
        enrollment.Final = request.Final;
        enrollment.MakeUp = request.MakeUp;
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    // Exams

    [HttpGet("term-courses/{courseId:int}/exams")]
    public async Task<ActionResult<IEnumerable<ExamDto>>> GetExams(int courseId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var exams = await _context.Exams
            .Where(e => e.CourseId == courseId)
            .OrderBy(e => e.Date)
            .Select(e => new ExamDto
            {
                Id = e.Id, ExamType = e.ExamType, ExamMethod = e.ExamMethod,
                Date = e.Date, QuestionCount = e.QuestionCount, Description = e.Description
            })
            .ToListAsync();
        return Ok(exams);
    }

    [HttpPost("term-courses/{courseId:int}/exams")]
    public async Task<ActionResult<ExamDto>> AddExam(int courseId, SaveExamRequest request)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var exam = new Exam
        {
            CourseId = courseId, ExamType = request.ExamType, ExamMethod = request.ExamMethod,
            Date = request.Date, QuestionCount = request.QuestionCount, Description = request.Description
        };
        _context.Exams.Add(exam);
        await _context.SaveChangesAsync();
        return Ok(new ExamDto
        {
            Id = exam.Id, ExamType = exam.ExamType, ExamMethod = exam.ExamMethod,
            Date = exam.Date, QuestionCount = exam.QuestionCount, Description = exam.Description
        });
    }

    [HttpPut("term-courses/{courseId:int}/exams/{examId:int}")]
    public async Task<IActionResult> UpdateExam(int courseId, int examId, SaveExamRequest request)
    {
        if (!await OwnsCourse(courseId)) return Forbid();
        var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == examId && e.CourseId == courseId);
        if (exam == null) return NotFound();

        exam.ExamType = request.ExamType;
        exam.ExamMethod = request.ExamMethod;
        exam.Date = request.Date;
        exam.QuestionCount = request.QuestionCount;
        exam.Description = request.Description;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("term-courses/{courseId:int}/exams/{examId:int}")]
    public async Task<IActionResult> DeleteExam(int courseId, int examId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();
        var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == examId && e.CourseId == courseId);
        if (exam == null) return NotFound();
        _context.Exams.Remove(exam);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Assessment Components

    [HttpGet("term-courses/{courseId:int}/assessment-components")]
    public async Task<ActionResult<IEnumerable<AssessmentComponentDto>>> GetAssessmentComponents(int courseId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var components = await _context.AssessmentComponents
            .Where(a => a.CourseId == courseId)
            .Select(a => new AssessmentComponentDto
            {
                Id = a.Id, Name = a.Name, Type = a.Type,
                Weight = a.Weight, Date = a.Date, Description = a.Description
            })
            .ToListAsync();
        return Ok(components);
    }

    [HttpPost("term-courses/{courseId:int}/assessment-components")]
    public async Task<ActionResult<AssessmentComponentDto>> AddAssessmentComponent(int courseId, SaveAssessmentComponentRequest request)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var component = new AssessmentComponent
        {
            CourseId = courseId, Name = request.Name, Type = request.Type,
            Weight = request.Weight, Date = request.Date, Description = request.Description
        };
        _context.AssessmentComponents.Add(component);
        await _context.SaveChangesAsync();
        return Ok(new AssessmentComponentDto
        {
            Id = component.Id, Name = component.Name, Type = component.Type,
            Weight = component.Weight, Date = component.Date, Description = component.Description
        });
    }

    [HttpPut("term-courses/{courseId:int}/assessment-components/{componentId:int}")]
    public async Task<IActionResult> UpdateAssessmentComponent(int courseId, int componentId, SaveAssessmentComponentRequest request)
    {
        if (!await OwnsCourse(courseId)) return Forbid();
        var component = await _context.AssessmentComponents
            .FirstOrDefaultAsync(a => a.Id == componentId && a.CourseId == courseId);
        if (component == null) return NotFound();

        component.Name = request.Name;
        component.Type = request.Type;
        component.Weight = request.Weight;
        component.Date = request.Date;
        component.Description = request.Description;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("term-courses/{courseId:int}/assessment-components/{componentId:int}")]
    public async Task<IActionResult> DeleteAssessmentComponent(int courseId, int componentId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();
        var component = await _context.AssessmentComponents
            .FirstOrDefaultAsync(a => a.Id == componentId && a.CourseId == courseId);
        if (component == null) return NotFound();
        _context.AssessmentComponents.Remove(component);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Risk Analysis

    [HttpGet("term-courses/{courseId:int}/risk-analysis")]
    public async Task<ActionResult<IEnumerable<RiskAnalysisDto>>> GetRiskAnalysis(int courseId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var enrollments = await _context.Enrollments
            .Include(e => e.Student)
            .Where(e => e.CourseId == courseId)
            .ToListAsync();

        var result = enrollments.Select(e =>
        {
            var grades = new List<decimal?> { e.Midterm, e.Final }.Where(g => g.HasValue).Select(g => g!.Value).ToList();
            decimal? avg = grades.Any() ? grades.Average() : null;

            string risk;
            string suggestion;

            if (!avg.HasValue)
            {
                risk = "Orta";
                suggestion = "Henüz not girilmemiş, izlenmesi önerilir.";
            }
            else if (avg.Value < 40)
            {
                risk = "Yüksek";
                suggestion = "Not ortalaması çok düşük, akademik destek gerekebilir.";
            }
            else if (avg.Value < 60)
            {
                risk = "Orta";
                suggestion = "Sınır düzeyde performans, yakından takip edilmeli.";
            }
            else
            {
                risk = "Düşük";
                suggestion = "Performans yeterli düzeyde.";
            }

            return new RiskAnalysisDto
            {
                StudentId = e.StudentId,
                StudentNo = e.Student.StudentNumber,
                FullName = e.Student.FirstName + " " + e.Student.LastName,
                GradeAverage = avg,
                RiskLevel = risk,
                Suggestion = suggestion
            };
        }).ToList();

        return Ok(result);
    }

    // ── Yardımcı metotlar ────────────────────────────────────────────────────

     private static bool IsValidGrade(decimal? grade) =>     // ← YENİ, buraya ekle
        grade == null || (grade >= 0 && grade <= 100);

    private static CourseDetailDto MapToCourseDetailDto(Course course) => new()
    {
        Id = course.Id,
        Code = course.Code,
        Name = course.Name,
        Semester = course.Semester,
        Credit = course.Credit,
        Akts = course.Akts,
        WeeklyHours = course.WeeklyHours,
        IsMandatory = course.IsMandatory,
        Department = course.Department,
        ClassYear = course.ClassYear,
        CourseType = course.CourseType,
        IsLocked = course.IsLocked,
        TopicCount = course.CourseTopics.Count,
        LearningOutcomeCount = course.LearningOutcomes.Count,
        InstructorName = course.Instructor?.FullName,
        Description = course.Description,
        Objective = course.Objective
    };
}
