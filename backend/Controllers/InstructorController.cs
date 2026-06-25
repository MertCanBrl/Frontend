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

    private async Task<bool> OwnsUnlockedCourse(int courseId) =>
        await _context.Courses.AnyAsync(c => c.Id == courseId && c.InstructorId == GetUserId() && !c.IsLocked);

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
                ContentStatus = c.ContentStatus,
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
                ContentStatus = c.ContentStatus,
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
        if (course.IsLocked) return Forbid();

        course.Description = request.Description;
        course.Objective = request.Objective;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("course-contents/{courseId:int}/submit-for-review")]
    public async Task<IActionResult> SubmitForReview(int courseId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return NotFound();

        if (course.ContentStatus != "Draft" && course.ContentStatus != "RevisionRequested")
            return Conflict("Ders zaten onay sürecinde veya onaylanmış.");

        course.ContentStatus = "PendingApproval";
        course.IsLocked = true;
        course.SubmittedAt = DateTime.UtcNow;
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
        if (!await OwnsUnlockedCourse(courseId)) return Forbid();

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
        if (!await OwnsUnlockedCourse(courseId)) return Forbid();

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
        if (!await OwnsUnlockedCourse(courseId)) return Forbid();

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
                ContentStatus = c.ContentStatus,
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

    [HttpGet("term-courses/{courseId:int}/students")]
    public async Task<ActionResult<IEnumerable<StudentCourseResultDto>>> GetStudents(int courseId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var enrollments = await _context.Enrollments
            .Include(e => e.Student)
            .Where(e => e.CourseId == courseId)
            .OrderBy(e => e.Student.StudentNumber)
            .ToListAsync();

        var examGrades = await _context.ExamStudentGrades
            .Include(g => g.Exam)
            .Where(g => g.Exam.CourseId == courseId)
            .ToListAsync();

        // Dersin tüm bileşenleri
        var allComponents = await _context.AssessmentComponents
            .Where(a => a.CourseId == courseId)
            .OrderBy(a => a.Id)
            .ToListAsync();

        var avgComponents = allComponents.Where(c => c.IsIncludedInAverage).ToList();

        // Tüm bileşen notları
        var componentIds = allComponents.Select(c => c.Id).ToList();
        var allComponentGrades = componentIds.Any()
            ? await _context.AssessmentComponentStudentGrades
                .Where(g => componentIds.Contains(g.AssessmentComponentId))
                .ToListAsync()
            : new List<AssessmentComponentStudentGrade>();

        // (studentId -> (componentId -> score))
        var gradesByStudent = allComponentGrades
            .GroupBy(g => g.StudentId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.AssessmentComponentId, x => x.Score));

        var courseExams = await _context.Exams
            .Where(e => e.CourseId == courseId)
            .ToListAsync();

        var students = enrollments.Select(e =>
        {
            var sg = examGrades.Where(g => g.StudentId == e.StudentId).ToList();

            ExamStudentGrade? LatestExamGrade(string examType) =>
                sg.Where(g => g.Exam.ExamType == examType)
                  .OrderByDescending(g => g.Exam.Date ?? DateTime.MinValue)
                  .FirstOrDefault();

            var vizeGrade   = LatestExamGrade("Vize");
            var finalGrade  = LatestExamGrade("Final");
            var makeupGrade = LatestExamGrade("Bütünleme");

            gradesByStudent.TryGetValue(e.StudentId, out var studentGradeMap);
            studentGradeMap ??= new Dictionary<int, decimal>();

            // Her bileşen için öğrenci notu (null = girilmemiş)
            var componentScores = allComponents.Select(comp => new StudentComponentScoreDto
            {
                ComponentId   = comp.Id,
                ComponentName = comp.Name,
                Score         = studentGradeMap.TryGetValue(comp.Id, out var s) ? s : null,
                MaxScore      = comp.MaxScore
            }).ToList();

            // Ortalamaya dahil edilmiş ama notu girilmemiş bileşenler
            var missingNames = avgComponents
                .Where(c => !studentGradeMap.ContainsKey(c.Id))
                .Select(c => c.Name)
                .ToList();

            decimal? weightedAvg = ComputeWeightedAverage(
                e.StudentId, vizeGrade, finalGrade, makeupGrade,
                allComponentGrades, courseExams, avgComponents);

            return new StudentCourseResultDto
            {
                StudentId    = e.StudentId,
                StudentNo    = e.Student.StudentNumber,
                FullName     = e.Student.FirstName + " " + e.Student.LastName,
                Email        = e.Student.Email,
                Midterm      = vizeGrade?.TotalScore,
                Final        = finalGrade?.TotalScore,
                MakeUp       = makeupGrade?.TotalScore,
                WeightedAverage      = weightedAvg,
                ComponentScores      = componentScores,
                HasMissingGrades     = missingNames.Any(),
                MissingComponentNames = missingNames,
            };
        }).ToList();

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
            .Include(e => e.Questions)
            .Where(e => e.CourseId == courseId)
            .OrderBy(e => e.Date)
            .ToListAsync();

        var examIds = exams.Select(e => e.Id).ToList();
        var totalStudents = await _context.Enrollments.CountAsync(e => e.CourseId == courseId);
        var gradeCounts = await _context.ExamStudentGrades
            .Where(g => examIds.Contains(g.ExamId))
            .GroupBy(g => g.ExamId)
            .Select(g => new { ExamId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ExamId, x => x.Count);

        return Ok(exams.Select(e => new ExamDto
        {
            Id = e.Id,
            ExamType = e.ExamType,
            ExamMethod = e.ExamMethod,
            Date = e.Date,
            QuestionCount = e.QuestionCount,
            Description = e.Description,
            TotalScore = e.Questions.Sum(q => q.Score),
            WeightPercentage = e.WeightPercentage,
            HasGrades = gradeCounts.ContainsKey(e.Id),
            GradedStudentCount = gradeCounts.TryGetValue(e.Id, out var cnt) ? cnt : 0,
            TotalStudentCount = totalStudents
        }));
    }

    [HttpGet("term-courses/{courseId:int}/exams/{examId:int}")]
    public async Task<ActionResult<ExamDetailDto>> GetExamDetail(int courseId, int examId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var exam = await _context.Exams
            .Include(e => e.Questions)
                .ThenInclude(q => q.LearningOutcomeMappings)
            .FirstOrDefaultAsync(e => e.Id == examId && e.CourseId == courseId);

        if (exam == null) return NotFound();

        return Ok(new ExamDetailDto
        {
            Id = exam.Id,
            ExamType = exam.ExamType,
            ExamMethod = exam.ExamMethod,
            Date = exam.Date,
            QuestionCount = exam.QuestionCount,
            Description = exam.Description,
            TotalScore = exam.Questions.Sum(q => q.Score),
            Questions = exam.Questions
                .OrderBy(q => q.QuestionNumber)
                .Select(q => new ExamQuestionDto
                {
                    Id = q.Id,
                    QuestionNumber = q.QuestionNumber,
                    Description = q.Description,
                    Score = q.Score,
                    Difficulty = q.Difficulty,
                    BookletAQuestionNumber = q.BookletAQuestionNumber,
                    BookletBQuestionNumber = q.BookletBQuestionNumber,
                    BookletCQuestionNumber = q.BookletCQuestionNumber,
                    BookletDQuestionNumber = q.BookletDQuestionNumber,
                    LearningOutcomeWeights = q.LearningOutcomeMappings.Select(m => new LearningOutcomeWeightDto
                    {
                        LearningOutcomeId = m.LearningOutcomeId,
                        WeightPercentage = m.WeightPercentage
                    }).ToList()
                }).ToList()
        });
    }

    [HttpPost("term-courses/{courseId:int}/exams")]
    public async Task<ActionResult<ExamDetailDto>> AddExam(int courseId, SaveExamRequest request)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var contentStatus = await _context.Courses
            .Where(c => c.Id == courseId)
            .Select(c => c.ContentStatus)
            .FirstAsync();
        if (contentStatus != "Approved")
            return StatusCode(403, "Sınav oluşturmak için ders içeriğinin onaylanmış olması gerekir.");

        var exam = new Exam
        {
            CourseId = courseId,
            ExamType = request.ExamType,
            ExamMethod = request.ExamMethod,
            Date = ToUtc(request.Date),
            QuestionCount = request.QuestionCount,
            Description = request.Description,
            WeightPercentage = request.WeightPercentage,
        };
        _context.Exams.Add(exam);
        await _context.SaveChangesAsync();

        if (request.Questions?.Count > 0)
        {
            var err = await SaveExamQuestions(exam.Id, courseId, request.Questions);
            if (err != null) return BadRequest(new { message = err });
        }

        await _context.SaveChangesAsync();

        var created = await _context.Exams
            .Include(e => e.Questions).ThenInclude(q => q.LearningOutcomeMappings)
            .FirstAsync(e => e.Id == exam.Id);

        return Ok(MapToExamDetailDto(created));
    }

    [HttpPut("term-courses/{courseId:int}/exams/{examId:int}")]
    public async Task<ActionResult<ExamDetailDto>> UpdateExam(int courseId, int examId, SaveExamRequest request)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var exam = await _context.Exams
            .Include(e => e.Questions).ThenInclude(q => q.LearningOutcomeMappings)
            .FirstOrDefaultAsync(e => e.Id == examId && e.CourseId == courseId);
        if (exam == null) return NotFound();

        var hasGrades = await _context.ExamStudentGrades.AnyAsync(g => g.ExamId == examId);

        if (hasGrades)
        {
            var newCount = request.Questions?.Count ?? 0;
            if (newCount != exam.Questions.Count)
                return BadRequest(new { message = "Bu sınava not girişi yapıldığı için soru sayısı değiştirilemez." });

            // Not girilmiş sınavda sadece temel bilgiler + soru açıklamaları güncellenebilir
            exam.ExamType = request.ExamType;
            exam.ExamMethod = request.ExamMethod;
            exam.Date = ToUtc(request.Date);
            exam.QuestionCount = request.QuestionCount;
            exam.Description = request.Description;
            exam.WeightPercentage = request.WeightPercentage;

            if (request.Questions?.Count > 0)
            {
                var sorted = exam.Questions.OrderBy(q => q.QuestionNumber).ToList();
                var incoming = request.Questions.OrderBy(q => q.QuestionNumber).ToList();
                for (var i = 0; i < sorted.Count && i < incoming.Count; i++)
                    sorted[i].Description = incoming[i].Description;
            }
            await _context.SaveChangesAsync();
        }
        else
        {
            exam.ExamType = request.ExamType;
            exam.ExamMethod = request.ExamMethod;
            exam.Date = ToUtc(request.Date);
            exam.QuestionCount = request.QuestionCount;
            exam.Description = request.Description;
            exam.WeightPercentage = request.WeightPercentage;

            _context.ExamQuestions.RemoveRange(exam.Questions);
            exam.Questions.Clear();
            await _context.SaveChangesAsync();

            if (request.Questions?.Count > 0)
            {
                var err = await SaveExamQuestions(exam.Id, courseId, request.Questions);
                if (err != null) return BadRequest(new { message = err });
            }

            await _context.SaveChangesAsync();
        }

        var updated = await _context.Exams
            .Include(e => e.Questions).ThenInclude(q => q.LearningOutcomeMappings)
            .FirstAsync(e => e.Id == exam.Id);

        return Ok(MapToExamDetailDto(updated));
    }

    [HttpGet("term-courses/{courseId:int}/exams/{examId:int}/grade-entry")]
    public async Task<ActionResult<ExamGradeEntryDto>> GetExamGradeEntry(int courseId, int examId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var exam = await _context.Exams
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Id == examId && e.CourseId == courseId);
        if (exam == null) return NotFound();

        var enrollments = await _context.Enrollments
            .Include(e => e.Student)
            .Where(e => e.CourseId == courseId)
            .OrderBy(e => e.Student.StudentNumber)
            .ToListAsync();

        var grades = await _context.ExamStudentGrades
            .Include(g => g.QuestionScores)
            .Where(g => g.ExamId == examId)
            .ToListAsync();

        var gradesByStudentId = grades.ToDictionary(g => g.StudentId);
        var questions = exam.Questions.OrderBy(q => q.QuestionNumber).ToList();

        var students = enrollments.Select(e =>
        {
            gradesByStudentId.TryGetValue(e.StudentId, out var grade);
            var qScores = questions.Select(q =>
            {
                var qs = grade?.QuestionScores.FirstOrDefault(s => s.ExamQuestionId == q.Id);
                return new GradeEntryQuestionScoreDto { QuestionId = q.Id, Score = qs?.Score };
            }).ToList();

            return new GradeEntryStudentDto
            {
                StudentId = e.StudentId,
                StudentNumber = e.Student.StudentNumber,
                FullName = $"{e.Student.FirstName} {e.Student.LastName}",
                TotalScore = grade?.TotalScore ?? 0,
                IsCompleted = grade?.IsCompleted ?? false,
                QuestionScores = qScores
            };
        }).ToList();

        return Ok(new ExamGradeEntryDto
        {
            ExamId = exam.Id,
            CourseId = courseId,
            ExamType = exam.ExamType,
            ExamMethod = exam.ExamMethod,
            Date = exam.Date,
            Description = exam.Description,
            Questions = questions.Select(q => new GradeEntryQuestionDto
            {
                Id = q.Id,
                QuestionNumber = q.QuestionNumber,
                Description = q.Description,
                MaxScore = q.Score
            }).ToList(),
            Students = students
        });
    }

    [HttpPut("term-courses/{courseId:int}/exams/{examId:int}/grade-entry")]
    public async Task<IActionResult> SaveExamGradeEntry(int courseId, int examId, SaveExamGradeEntryRequest request)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var exam = await _context.Exams
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Id == examId && e.CourseId == courseId);
        if (exam == null) return NotFound();

        var validStudentIds = await _context.Enrollments
            .Where(e => e.CourseId == courseId)
            .Select(e => e.StudentId)
            .ToHashSetAsync();

        var validQuestions = exam.Questions.ToDictionary(q => q.Id, q => q.Score);

        // Validation
        foreach (var s in request.Students)
        {
            if (!validStudentIds.Contains(s.StudentId))
                return BadRequest(new { message = $"Öğrenci (Id: {s.StudentId}) bu derse kayıtlı değil." });

            foreach (var qs in s.QuestionScores)
            {
                if (!validQuestions.TryGetValue(qs.QuestionId, out var maxScore))
                    return BadRequest(new { message = $"Soru (Id: {qs.QuestionId}) bu sınava ait değil." });
                if (qs.Score < 0)
                    return BadRequest(new { message = $"Soru {qs.QuestionId} puanı negatif olamaz." });
                if (qs.Score > maxScore)
                    return BadRequest(new { message = $"Soru {qs.QuestionId} için girilen puan ({qs.Score}), sorunun maksimum puanını ({maxScore}) aşıyor." });
            }
        }

        try
        {
            foreach (var s in request.Students)
            {
                var grade = await _context.ExamStudentGrades
                    .Include(g => g.QuestionScores)
                    .FirstOrDefaultAsync(g => g.ExamId == examId && g.StudentId == s.StudentId);

                if (grade == null)
                {
                    grade = new ExamStudentGrade
                    {
                        ExamId = examId,
                        StudentId = s.StudentId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.ExamStudentGrades.Add(grade);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _context.ExamQuestionStudentScores.RemoveRange(grade.QuestionScores);
                    await _context.SaveChangesAsync();
                    grade.UpdatedAt = DateTime.UtcNow;
                }

                decimal total = 0;
                foreach (var qs in s.QuestionScores)
                {
                    _context.ExamQuestionStudentScores.Add(new ExamQuestionStudentScore
                    {
                        ExamStudentGradeId = grade.Id,
                        ExamQuestionId = qs.QuestionId,
                        Score = qs.Score,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    total += qs.Score;
                }

                grade.TotalScore = total;
                grade.IsCompleted = s.QuestionScores.Count == exam.Questions.Count;
                await _context.SaveChangesAsync();
            }

            var gradedCount = await _context.ExamStudentGrades.CountAsync(g => g.ExamId == examId);
            return Ok(new { message = "Notlar başarıyla kaydedildi.", gradedStudentCount = gradedCount });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SaveExamGradeEntry error: {ex.Message}\n{ex.InnerException?.Message}");
            return StatusCode(500, new { message = "Notlar kaydedilirken bir hata oluştu.", detail = ex.InnerException?.Message ?? ex.Message });
        }
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

    private async Task<string?> SaveExamQuestions(int examId, int courseId, List<SaveExamQuestionRequest> questionRequests)
    {
        var validLoIds = await _context.LearningOutcomes
            .Where(lo => lo.CourseId == courseId)
            .Select(lo => lo.Id)
            .ToHashSetAsync();

        // Validate all questions before writing anything
        foreach (var qr in questionRequests)
        {
            if (qr.LearningOutcomeWeights.Count == 0)
                return $"Soru {qr.QuestionNumber}: Her sınav sorusu en az bir Öğrenme Çıktısı ile eşleştirilmelidir.";
            var err = ValidateLearningOutcomeWeights(qr.LearningOutcomeWeights, validLoIds);
            if (err != null) return $"Soru {qr.QuestionNumber}: {err}";
        }

        foreach (var qr in questionRequests)
        {
            var question = new ExamQuestion
            {
                ExamId = examId,
                QuestionNumber = qr.QuestionNumber,
                Description = qr.Description,
                Score = qr.Score,
                Difficulty = qr.Difficulty,
                BookletAQuestionNumber = qr.BookletAQuestionNumber,
                BookletBQuestionNumber = qr.BookletBQuestionNumber,
                BookletCQuestionNumber = qr.BookletCQuestionNumber,
                BookletDQuestionNumber = qr.BookletDQuestionNumber
            };
            _context.ExamQuestions.Add(question);
            await _context.SaveChangesAsync();

            foreach (var w in qr.LearningOutcomeWeights)
            {
                _context.ExamQuestionLearningOutcomes.Add(new ExamQuestionLearningOutcome
                {
                    ExamQuestionId = question.Id,
                    LearningOutcomeId = w.LearningOutcomeId,
                    WeightPercentage = w.WeightPercentage
                });
            }
        }
        return null;
    }

    private static string? ValidateLearningOutcomeWeights(
        List<SaveLearningOutcomeWeightRequest> weights,
        IEnumerable<int> validLoIds)
    {
        if (weights.Count == 0) return null;

        var validSet = validLoIds.ToHashSet();
        var seenIds = new HashSet<int>();

        foreach (var w in weights)
        {
            if (!validSet.Contains(w.LearningOutcomeId))
                return $"Öğrenme çıktısı ID {w.LearningOutcomeId} bu derse ait değil.";
            if (!seenIds.Add(w.LearningOutcomeId))
                return $"Öğrenme çıktısı ID {w.LearningOutcomeId} birden fazla kez belirtilmiş.";
            if (w.WeightPercentage <= 0 || w.WeightPercentage > 100)
                return "Öğrenme çıktısı ağırlığı 0'dan büyük, 100'den küçük veya eşit olmalıdır.";
        }

        var total = weights.Sum(w => w.WeightPercentage);
        if (Math.Abs(total - 100m) > 0.01m)
            return $"Öğrenme çıktıları ağırlıkları toplamı 100 olmalıdır (şu an: {total:F2}).";

        return null;
    }

    private static ExamDetailDto MapToExamDetailDto(Exam exam) => new()
    {
        Id = exam.Id,
        ExamType = exam.ExamType,
        ExamMethod = exam.ExamMethod,
        Date = exam.Date,
        QuestionCount = exam.QuestionCount,
        Description = exam.Description,
        TotalScore = exam.Questions.Sum(q => q.Score),
        WeightPercentage = exam.WeightPercentage,
        Questions = exam.Questions
            .OrderBy(q => q.QuestionNumber)
            .Select(q => new ExamQuestionDto
            {
                Id = q.Id,
                QuestionNumber = q.QuestionNumber,
                Description = q.Description,
                Score = q.Score,
                Difficulty = q.Difficulty,
                BookletAQuestionNumber = q.BookletAQuestionNumber,
                BookletBQuestionNumber = q.BookletBQuestionNumber,
                BookletCQuestionNumber = q.BookletCQuestionNumber,
                BookletDQuestionNumber = q.BookletDQuestionNumber,
                LearningOutcomeWeights = q.LearningOutcomeMappings.Select(m => new LearningOutcomeWeightDto
                {
                    LearningOutcomeId = m.LearningOutcomeId,
                    WeightPercentage = m.WeightPercentage
                }).ToList()
            }).ToList()
    };

    // Assessment Components

    [HttpGet("term-courses/{courseId:int}/assessment-components")]
    public async Task<ActionResult<IEnumerable<AssessmentComponentDto>>> GetAssessmentComponents(int courseId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var components = await _context.AssessmentComponents
            .Include(a => a.LearningOutcomeMappings)
            .Where(a => a.CourseId == courseId)
            .OrderBy(a => a.Id)
            .ToListAsync();

        return Ok(components.Select(a => new AssessmentComponentDto
        {
            Id = a.Id, Name = a.Name, Type = a.Type,
            Weight = a.Weight, Date = a.Date, Description = a.Description,
            MaxScore = a.MaxScore,
            IsIncludedInAverage = a.IsIncludedInAverage,
            GradeGroup = a.GradeGroup,
            GroupWeightPercentage = a.GroupWeightPercentage,
            LearningOutcomeWeights = a.LearningOutcomeMappings.Select(m => new LearningOutcomeWeightDto
            {
                LearningOutcomeId = m.LearningOutcomeId,
                WeightPercentage = m.WeightPercentage
            }).ToList()
        }));
    }

    [HttpPost("term-courses/{courseId:int}/assessment-components")]
    public async Task<ActionResult<AssessmentComponentDto>> AddAssessmentComponent(int courseId, SaveAssessmentComponentRequest request)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        if (request.MaxScore <= 0)
            return BadRequest(new { message = "Maksimum puan 0'dan büyük olmalıdır." });
        if (request.GroupWeightPercentage < 0 || request.GroupWeightPercentage > 100)
            return BadRequest(new { message = "Grup içi ağırlık 0 ile 100 arasında olmalıdır." });
        if (request.IsIncludedInAverage)
        {
            if (string.IsNullOrWhiteSpace(request.GradeGroup))
                return BadRequest(new { message = "Ortalamaya dahil bir bileşen için not grubu seçilmelidir." });
            if (request.GroupWeightPercentage <= 0)
                return BadRequest(new { message = "Ortalamaya dahil bir bileşen için grup içi ağırlık 0'dan büyük olmalıdır." });
            var existingGroupWeight = await _context.AssessmentComponents
                .Where(a => a.CourseId == courseId && a.IsIncludedInAverage && a.GradeGroup == request.GradeGroup)
                .SumAsync(a => a.GroupWeightPercentage);
            if (existingGroupWeight + request.GroupWeightPercentage > 100)
                return BadRequest(new { message = $"'{request.GradeGroup}' grubunun toplam bileşen ağırlığı 100%'ü aşıyor (mevcut: {existingGroupWeight}%, eklenecek: {request.GroupWeightPercentage}%)." });
        }

        // Validate LO weights before any DB write
        if (request.IsIncludedInAverage && request.LearningOutcomeWeights.Count == 0)
            return BadRequest(new { message = "Ortalamaya dahil edilen ölçme bileşeni en az bir Öğrenme Çıktısı ile eşleştirilmelidir." });
        var validLoIds = await _context.LearningOutcomes
            .Where(lo => lo.CourseId == courseId)
            .Select(lo => lo.Id)
            .ToHashSetAsync();
        var loErr = ValidateLearningOutcomeWeights(request.LearningOutcomeWeights, validLoIds);
        if (loErr != null) return BadRequest(new { message = loErr });

        var component = new AssessmentComponent
        {
            CourseId = courseId, Name = request.Name, Type = request.Type,
            Weight = request.Weight, Date = ToUtc(request.Date), Description = request.Description,
            MaxScore = request.MaxScore,
            IsIncludedInAverage = request.IsIncludedInAverage,
            GradeGroup = request.GradeGroup,
            GroupWeightPercentage = request.GroupWeightPercentage,
        };
        _context.AssessmentComponents.Add(component);
        await _context.SaveChangesAsync();

        await SyncComponentLearningOutcomes(component.Id, request.LearningOutcomeWeights);

        return Ok(new AssessmentComponentDto
        {
            Id = component.Id, Name = component.Name, Type = component.Type,
            Weight = component.Weight, Date = component.Date, Description = component.Description,
            MaxScore = component.MaxScore,
            IsIncludedInAverage = component.IsIncludedInAverage,
            GradeGroup = component.GradeGroup,
            GroupWeightPercentage = component.GroupWeightPercentage,
            LearningOutcomeWeights = request.LearningOutcomeWeights.Select(w => new LearningOutcomeWeightDto
            {
                LearningOutcomeId = w.LearningOutcomeId,
                WeightPercentage = w.WeightPercentage
            }).ToList()
        });
    }

    [HttpPut("term-courses/{courseId:int}/assessment-components/{componentId:int}")]
    public async Task<IActionResult> UpdateAssessmentComponent(int courseId, int componentId, SaveAssessmentComponentRequest request)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        if (request.MaxScore <= 0)
            return BadRequest(new { message = "Maksimum puan 0'dan büyük olmalıdır." });
        if (request.GroupWeightPercentage < 0 || request.GroupWeightPercentage > 100)
            return BadRequest(new { message = "Grup içi ağırlık 0 ile 100 arasında olmalıdır." });
        if (request.IsIncludedInAverage)
        {
            if (string.IsNullOrWhiteSpace(request.GradeGroup))
                return BadRequest(new { message = "Ortalamaya dahil bir bileşen için not grubu seçilmelidir." });
            if (request.GroupWeightPercentage <= 0)
                return BadRequest(new { message = "Ortalamaya dahil bir bileşen için grup içi ağırlık 0'dan büyük olmalıdır." });
            var existingGroupWeight = await _context.AssessmentComponents
                .Where(a => a.CourseId == courseId && a.IsIncludedInAverage && a.GradeGroup == request.GradeGroup && a.Id != componentId)
                .SumAsync(a => a.GroupWeightPercentage);
            if (existingGroupWeight + request.GroupWeightPercentage > 100)
                return BadRequest(new { message = $"'{request.GradeGroup}' grubunun toplam bileşen ağırlığı 100%'ü aşıyor (mevcut: {existingGroupWeight}%, bu bileşen: {request.GroupWeightPercentage}%)." });
        }

        // Validate LO weights before any DB write
        if (request.IsIncludedInAverage && request.LearningOutcomeWeights.Count == 0)
            return BadRequest(new { message = "Ortalamaya dahil edilen ölçme bileşeni en az bir Öğrenme Çıktısı ile eşleştirilmelidir." });
        var validLoIds2 = await _context.LearningOutcomes
            .Where(lo => lo.CourseId == courseId)
            .Select(lo => lo.Id)
            .ToHashSetAsync();
        var loErr2 = ValidateLearningOutcomeWeights(request.LearningOutcomeWeights, validLoIds2);
        if (loErr2 != null) return BadRequest(new { message = loErr2 });

        var component = await _context.AssessmentComponents
            .Include(a => a.LearningOutcomeMappings)
            .FirstOrDefaultAsync(a => a.Id == componentId && a.CourseId == courseId);
        if (component == null) return NotFound();

        component.Name = request.Name;
        component.Type = request.Type;
        component.Weight = request.Weight;
        component.Date = ToUtc(request.Date);
        component.Description = request.Description;
        component.MaxScore = request.MaxScore;
        component.IsIncludedInAverage = request.IsIncludedInAverage;
        component.GradeGroup = request.GradeGroup;
        component.GroupWeightPercentage = request.GroupWeightPercentage;
        await _context.SaveChangesAsync();

        await SyncComponentLearningOutcomes(componentId, request.LearningOutcomeWeights);

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

    [HttpGet("term-courses/{courseId:int}/assessment-components/{componentId:int}/grade-entry")]
    public async Task<ActionResult<ComponentGradeEntryDto>> GetComponentGradeEntry(int courseId, int componentId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var component = await _context.AssessmentComponents
            .FirstOrDefaultAsync(a => a.Id == componentId && a.CourseId == courseId);
        if (component == null) return NotFound();

        var enrollments = await _context.Enrollments
            .Include(e => e.Student)
            .Where(e => e.CourseId == courseId)
            .OrderBy(e => e.Student.StudentNumber)
            .ToListAsync();

        var grades = await _context.AssessmentComponentStudentGrades
            .Where(g => g.AssessmentComponentId == componentId)
            .ToDictionaryAsync(g => g.StudentId);

        return Ok(new ComponentGradeEntryDto
        {
            ComponentId = component.Id,
            Name = component.Name,
            Type = component.Type,
            MaxScore = component.MaxScore,
            Students = enrollments.Select(e =>
            {
                grades.TryGetValue(e.StudentId, out var grade);
                return new ComponentStudentGradeDto
                {
                    StudentId = e.StudentId,
                    StudentNumber = e.Student.StudentNumber,
                    FullName = e.Student.FirstName + " " + e.Student.LastName,
                    Score = grade?.Score,
                };
            }).ToList()
        });
    }

    [HttpPut("term-courses/{courseId:int}/assessment-components/{componentId:int}/grade-entry")]
    public async Task<IActionResult> SaveComponentGradeEntry(int courseId, int componentId, SaveComponentGradeEntryRequest request)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var component = await _context.AssessmentComponents
            .FirstOrDefaultAsync(a => a.Id == componentId && a.CourseId == courseId);
        if (component == null) return NotFound();

        var enrolledIds = await _context.Enrollments
            .Where(e => e.CourseId == courseId)
            .Select(e => e.StudentId)
            .ToHashSetAsync();

        var existingGrades = await _context.AssessmentComponentStudentGrades
            .Where(g => g.AssessmentComponentId == componentId)
            .ToDictionaryAsync(g => g.StudentId);

        var now = DateTime.UtcNow;

        foreach (var sr in request.Students)
        {
            if (!enrolledIds.Contains(sr.StudentId))
                return BadRequest(new { message = $"Öğrenci {sr.StudentId} bu dersi almıyor." });

            if (sr.Score.HasValue && (sr.Score.Value < 0 || sr.Score.Value > component.MaxScore))
                return BadRequest(new { message = $"Puan 0 ile {component.MaxScore} arasında olmalıdır." });

            if (existingGrades.TryGetValue(sr.StudentId, out var existing))
            {
                if (sr.Score.HasValue)
                {
                    existing.Score = sr.Score.Value;
                    existing.UpdatedAt = now;
                }
                else
                {
                    _context.AssessmentComponentStudentGrades.Remove(existing);
                }
            }
            else if (sr.Score.HasValue)
            {
                _context.AssessmentComponentStudentGrades.Add(new AssessmentComponentStudentGrade
                {
                    AssessmentComponentId = componentId,
                    StudentId = sr.StudentId,
                    Score = sr.Score.Value,
                    CreatedAt = now,
                    UpdatedAt = now,
                });
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Notlar kaydedildi." });
    }

    // ÖÇ Durum Tablosu

    [HttpGet("term-courses/{courseId:int}/learning-outcome-status")]
    public async Task<ActionResult<IEnumerable<LearningOutcomeStatusDto>>> GetLearningOutcomeStatus(int courseId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var learningOutcomes = await _context.LearningOutcomes
            .Where(lo => lo.CourseId == courseId)
            .OrderBy(lo => lo.Code)
            .ToListAsync();

        if (!learningOutcomes.Any()) return Ok(Array.Empty<LearningOutcomeStatusDto>());

        var loIds = learningOutcomes.Select(lo => lo.Id).ToHashSet();

        // Sınav soruları — ÖÇ eşleştirmeleri
        var questionMappings = await _context.ExamQuestionLearningOutcomes
            .Include(m => m.ExamQuestion).ThenInclude(q => q.Exam)
            .Where(m => loIds.Contains(m.LearningOutcomeId) && m.ExamQuestion.Exam.CourseId == courseId)
            .ToListAsync();

        var questionIds = questionMappings.Select(m => m.ExamQuestionId).Distinct().ToList();
        var questionScores = questionIds.Count > 0
            ? await _context.ExamQuestionStudentScores
                .Where(s => questionIds.Contains(s.ExamQuestionId))
                .ToListAsync()
            : [];

        // Ölçme bileşenleri — ÖÇ eşleştirmeleri
        var componentMappings = await _context.AssessmentComponentLearningOutcomes
            .Include(m => m.AssessmentComponent)
            .Where(m => loIds.Contains(m.LearningOutcomeId) && m.AssessmentComponent.CourseId == courseId)
            .ToListAsync();

        var compIds = componentMappings.Select(m => m.AssessmentComponentId).Distinct().ToList();
        var componentGrades = compIds.Count > 0
            ? await _context.AssessmentComponentStudentGrades
                .Where(g => compIds.Contains(g.AssessmentComponentId))
                .ToListAsync()
            : [];

        var result = learningOutcomes.Select(lo =>
        {
            var sources = new List<LoSourceDto>();

            foreach (var qm in questionMappings.Where(m => m.LearningOutcomeId == lo.Id))
            {
                var qScores = questionScores.Where(s => s.ExamQuestionId == qm.ExamQuestionId).ToList();
                if (!qScores.Any()) continue;

                var maxScore = qm.ExamQuestion.Score;
                var avgRaw = qScores.Average(s => s.Score);
                var avgNorm = maxScore > 0
                    ? Math.Round(avgRaw / maxScore * 100m, 1)
                    : null as decimal?;

                var desc = qm.ExamQuestion.Description;
                var label = desc?.Length > 35 ? desc[..35] + "…" : desc ?? "";
                sources.Add(new LoSourceDto
                {
                    SourceType = "ExamQuestion",
                    SourceName = $"S{qm.ExamQuestion.QuestionNumber}: {label}",
                    ExamType = qm.ExamQuestion.Exam.ExamType,
                    MaxRawScore = maxScore,
                    AverageRawScore = Math.Round(avgRaw, 2),
                    AverageNormalized = avgNorm,
                    LoWeightPercentage = qm.WeightPercentage,
                    StudentCount = qScores.Count
                });
            }

            foreach (var cm in componentMappings.Where(m => m.LearningOutcomeId == lo.Id))
            {
                var cGrades = componentGrades.Where(g => g.AssessmentComponentId == cm.AssessmentComponentId).ToList();
                if (!cGrades.Any()) continue;

                var maxScore = cm.AssessmentComponent.MaxScore;
                var avgRaw = cGrades.Average(g => g.Score);
                var avgNorm = maxScore > 0
                    ? Math.Round(avgRaw / maxScore * 100m, 1)
                    : null as decimal?;

                sources.Add(new LoSourceDto
                {
                    SourceType = "Component",
                    SourceName = cm.AssessmentComponent.Name,
                    ExamType = cm.AssessmentComponent.Type,
                    MaxRawScore = maxScore,
                    AverageRawScore = Math.Round(avgRaw, 2),
                    AverageNormalized = avgNorm,
                    LoWeightPercentage = cm.WeightPercentage,
                    StudentCount = cGrades.Count
                });
            }

            // Weighted success: Σ(avgRaw * loWeight) / Σ(maxRaw * loWeight) * 100
            decimal totalWeightedScore = 0m;
            decimal totalWeightedMax = 0m;
            foreach (var src in sources.Where(s => s.AverageRawScore.HasValue))
            {
                totalWeightedScore += src.AverageRawScore!.Value * src.LoWeightPercentage / 100m;
                totalWeightedMax += src.MaxRawScore * src.LoWeightPercentage / 100m;
            }
            var overallAvg = totalWeightedMax > 0
                ? Math.Round(totalWeightedScore / totalWeightedMax * 100m, 1)
                : null as decimal?;

            return new LearningOutcomeStatusDto
            {
                LearningOutcomeId = lo.Id,
                Code = lo.Code,
                Description = lo.Description,
                AverageSuccess = overallAvg,
                SourceCount = sources.Count,
                Sources = sources
            };
        }).ToList();

        return Ok(result);
    }

    // Dönem Sonu — Bileşen Raporu

    [HttpGet("term-courses/{courseId:int}/component-report")]
    public async Task<ActionResult<IEnumerable<ComponentReportItemDto>>> GetComponentReport(int courseId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var components = await _context.AssessmentComponents
            .Include(a => a.LearningOutcomeMappings)
            .Where(a => a.CourseId == courseId)
            .OrderBy(a => a.Id)
            .ToListAsync();

        if (!components.Any()) return Ok(Array.Empty<ComponentReportItemDto>());

        var totalStudents = await _context.Enrollments.CountAsync(e => e.CourseId == courseId);

        var compIds = components.Select(c => c.Id).ToList();
        var allGrades = await _context.AssessmentComponentStudentGrades
            .Where(g => compIds.Contains(g.AssessmentComponentId))
            .ToListAsync();

        var gradesByComp = allGrades
            .GroupBy(g => g.AssessmentComponentId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Score).ToList());

        return Ok(components.Select(c =>
        {
            gradesByComp.TryGetValue(c.Id, out var scores);
            scores ??= [];
            decimal? avgScore = scores.Count > 0 ? Math.Round(scores.Average(), 2) : null;
            decimal? avgNorm  = avgScore.HasValue && c.MaxScore > 0
                ? Math.Round(avgScore.Value / c.MaxScore * 100m, 1)
                : null;

            return new ComponentReportItemDto
            {
                ComponentId = c.Id,
                Name = c.Name,
                Type = c.Type,
                GradeGroup = c.GradeGroup,
                GroupWeightPercentage = c.GroupWeightPercentage,
                MaxScore = c.MaxScore,
                IsIncludedInAverage = c.IsIncludedInAverage,
                LearningOutcomeWeights = c.LearningOutcomeMappings.Select(m => new LearningOutcomeWeightDto
                {
                    LearningOutcomeId = m.LearningOutcomeId,
                    WeightPercentage = m.WeightPercentage
                }).ToList(),
                TotalStudents = totalStudents,
                GradedCount = scores.Count,
                AverageScore = avgScore,
                AverageNormalized = avgNorm
            };
        }));
    }

    private async Task SyncComponentLearningOutcomes(int componentId, List<SaveLearningOutcomeWeightRequest> loWeights)
    {
        var existing = await _context.AssessmentComponentLearningOutcomes
            .Where(m => m.AssessmentComponentId == componentId)
            .ToListAsync();
        _context.AssessmentComponentLearningOutcomes.RemoveRange(existing);

        foreach (var w in loWeights)
            _context.AssessmentComponentLearningOutcomes.Add(new AssessmentComponentLearningOutcome
            {
                AssessmentComponentId = componentId,
                LearningOutcomeId = w.LearningOutcomeId,
                WeightPercentage = w.WeightPercentage
            });

        await _context.SaveChangesAsync();
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

        var examGrades = await _context.ExamStudentGrades
            .Include(g => g.Exam)
            .Where(g => g.Exam.CourseId == courseId)
            .ToListAsync();

        var allComponents = await _context.AssessmentComponents
            .Where(a => a.CourseId == courseId)
            .ToListAsync();

        var avgComponents = allComponents.Where(c => c.IsIncludedInAverage).ToList();

        var componentIds = allComponents.Select(c => c.Id).ToList();
        var allComponentGrades = componentIds.Any()
            ? await _context.AssessmentComponentStudentGrades
                .Where(g => componentIds.Contains(g.AssessmentComponentId))
                .ToListAsync()
            : new List<AssessmentComponentStudentGrade>();

        var courseExams = await _context.Exams
            .Where(e => e.CourseId == courseId)
            .ToListAsync();

        var gradesByStudent = allComponentGrades
            .GroupBy(g => g.StudentId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.AssessmentComponentId, x => x.Score));

        var result = enrollments.Select(e =>
        {
            var sg = examGrades.Where(g => g.StudentId == e.StudentId).ToList();

            ExamStudentGrade? LatestExamGrade(string examType) =>
                sg.Where(g => g.Exam.ExamType == examType)
                  .OrderByDescending(g => g.Exam.Date ?? DateTime.MinValue)
                  .FirstOrDefault();

            var vizeGrade   = LatestExamGrade("Vize");
            var finalGrade  = LatestExamGrade("Final");
            var makeupGrade = LatestExamGrade("Bütünleme");

            gradesByStudent.TryGetValue(e.StudentId, out var studentGradeMap);
            studentGradeMap ??= new Dictionary<int, decimal>();

            var missingNames = avgComponents
                .Where(c => !studentGradeMap.ContainsKey(c.Id))
                .Select(c => c.Name)
                .ToList();

            var avg = ComputeWeightedAverage(
                e.StudentId, vizeGrade, finalGrade, makeupGrade,
                allComponentGrades, courseExams, avgComponents);

            bool hasMissing = missingNames.Any();
            string risk;
            string suggestion;

            if (!avg.HasValue)
            {
                risk = "Orta";
                suggestion = "Ağırlıklı ortalama hesaplanamadı; sınav ağırlıklarını ve not girişlerini kontrol edin.";
            }
            else if (hasMissing)
            {
                if (avg.Value < 40)      { risk = "Yüksek"; suggestion = $"Ortalama düşük ({avg.Value:F1}); eksik bileşen notları var."; }
                else if (avg.Value < 60) { risk = "Orta";   suggestion = $"Sınır düzey ({avg.Value:F1}); bazı bileşen notları eksik."; }
                else                     { risk = "Düşük";  suggestion = $"Performans yeterli ({avg.Value:F1}) ancak bazı bileşen notları eksik."; }
            }
            else if (avg.Value < 40)      { risk = "Yüksek"; suggestion = "Not ortalaması çok düşük, akademik destek gerekebilir."; }
            else if (avg.Value < 60)      { risk = "Orta";   suggestion = "Sınır düzeyde performans, yakından takip edilmeli."; }
            else                          { risk = "Düşük";  suggestion = "Performans yeterli düzeyde."; }

            return new RiskAnalysisDto
            {
                StudentId        = e.StudentId,
                StudentNo        = e.Student.StudentNumber,
                FullName         = e.Student.FirstName + " " + e.Student.LastName,
                GradeAverage     = avg,
                RiskLevel        = risk,
                Suggestion       = suggestion,
                HasMissingGrades = hasMissing
            };
        }).ToList();

        return Ok(result);
    }

    // Statistics (Dönem Sonu Raporları)

    [HttpGet("term-courses/{courseId:int}/statistics")]
    public async Task<ActionResult<CourseStatisticsDto>> GetStatistics(int courseId)
    {
        if (!await OwnsCourse(courseId)) return Forbid();

        var enrollmentStudentIds = await _context.Enrollments
            .Where(e => e.CourseId == courseId)
            .Select(e => e.StudentId)
            .ToListAsync();

        var examGrades = await _context.ExamStudentGrades
            .Include(g => g.Exam)
            .Where(g => g.Exam.CourseId == courseId)
            .ToListAsync();

        var allComponents = await _context.AssessmentComponents
            .Where(a => a.CourseId == courseId)
            .ToListAsync();

        var avgComponents = allComponents.Where(c => c.IsIncludedInAverage).ToList();

        var componentIds = allComponents.Select(c => c.Id).ToList();
        var allComponentGrades = componentIds.Any()
            ? await _context.AssessmentComponentStudentGrades
                .Where(g => componentIds.Contains(g.AssessmentComponentId))
                .ToListAsync()
            : new List<AssessmentComponentStudentGrade>();

        var courseExams = await _context.Exams
            .Where(e => e.CourseId == courseId)
            .ToListAsync();

        var successScores = new List<decimal>();
        foreach (var studentId in enrollmentStudentIds)
        {
            var sg = examGrades.Where(g => g.StudentId == studentId).ToList();

            var vizeGrade = sg
                .Where(g => g.Exam.ExamType == "Vize")
                .OrderByDescending(g => g.Exam.Date ?? DateTime.MinValue)
                .FirstOrDefault();
            var finalGrade = sg
                .Where(g => g.Exam.ExamType == "Final")
                .OrderByDescending(g => g.Exam.Date ?? DateTime.MinValue)
                .FirstOrDefault();
            var makeupGrade = sg
                .Where(g => g.Exam.ExamType == "Bütünleme")
                .OrderByDescending(g => g.Exam.Date ?? DateTime.MinValue)
                .FirstOrDefault();

            var score = ComputeWeightedAverage(
                studentId, vizeGrade, finalGrade, makeupGrade,
                allComponentGrades, courseExams, avgComponents);

            if (score.HasValue) successScores.Add(score.Value);
        }

        var buckets = new (string Label, decimal Min, decimal Max)[]
        {
            ("0-49",   0,  49.999m),
            ("50-59",  50, 59.999m),
            ("60-69",  60, 69.999m),
            ("70-84",  70, 84.999m),
            ("85-100", 85, 100m),
        };

        var distribution = buckets.Select(b => new GradeBucketDto
        {
            Label = b.Label,
            Count = successScores.Count(s => s >= b.Min && s <= b.Max)
        }).ToList();

        return Ok(new CourseStatisticsDto
        {
            TotalStudents  = enrollmentStudentIds.Count,
            GradedStudents = successScores.Count,
            ClassAverage   = successScores.Any() ? Math.Round(successScores.Average(), 2) : null,
            PassCount      = successScores.Count(s => s >= 50),
            FailCount      = successScores.Count(s => s < 50),
            Distribution   = distribution
        });
    }
    // ── Yardımcı metotlar ────────────────────────────────────────────────────

    private static bool IsValidGrade(decimal? grade) =>
        grade == null || (grade >= 0 && grade <= 100);

    // Merkezi ağırlıklı ortalama hesabı.
    // Grup skoru = (sınav * examShare) + Σ(bileşenScore/maxScore * compWeight%)
    // examShare = MAX(0, 100 - Σ(compWeight%)) / 100
    // Bütünleme notu varsa Final grubunda Final sınavı yerine kullanılır.
    // Not girilmemiş bileşen 0 puan olarak hesaba katılır (hasMissingGrades ile işaretlenir).
    private static decimal? ComputeWeightedAverage(
        int studentId,
        ExamStudentGrade? vizeGrade,
        ExamStudentGrade? finalGrade,
        ExamStudentGrade? makeupGrade,
        List<AssessmentComponentStudentGrade> allCompGrades,
        List<Exam> courseExams,
        List<AssessmentComponent> avgComponents)
    {
        decimal? ExamWeight(string examType) =>
            courseExams
                .Where(e => e.ExamType == examType && e.WeightPercentage.HasValue)
                .OrderByDescending(e => e.Date ?? DateTime.MinValue)
                .Select(e => e.WeightPercentage)
                .FirstOrDefault();

        var vizeWeight  = ExamWeight("Vize")  ?? 0m;
        var finalWeight = ExamWeight("Final") ?? 0m;

        if (vizeWeight == 0 && finalWeight == 0)
            return null;

        // Bu öğrencinin bileşen notu haritası: componentId -> score
        var studentGradeMap = allCompGrades
            .Where(g => g.StudentId == studentId)
            .ToDictionary(g => g.AssessmentComponentId, g => g.Score);

        decimal GroupScore(string gradeGroup, decimal? examScore)
        {
            var groupComps = avgComponents.Where(c => c.GradeGroup == gradeGroup).ToList();

            var totalCompWeight = groupComps.Sum(c => c.GroupWeightPercentage);
            var examShare = Math.Max(0m, 100m - totalCompWeight) / 100m;

            var compScore = groupComps.Sum(c =>
            {
                var score = studentGradeMap.TryGetValue(c.Id, out var s) ? s : 0m;
                return score / c.MaxScore * c.GroupWeightPercentage / 100m * 100m;
            });

            return (examScore ?? 0m) * examShare + compScore;
        }

        var vizeScore      = GroupScore("Midterm", vizeGrade?.TotalScore);
        var finalExamScore = makeupGrade?.TotalScore ?? finalGrade?.TotalScore;
        var finalScore     = GroupScore("Final", finalExamScore);

        return Math.Round(vizeScore * (vizeWeight / 100m) + finalScore * (finalWeight / 100m), 2);
    }

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
        ContentStatus = course.ContentStatus,
        TopicCount = course.CourseTopics.Count,
        LearningOutcomeCount = course.LearningOutcomes.Count,
        InstructorName = course.Instructor?.FullName,
        Description = course.Description,
        Objective = course.Objective,
        SubmittedAt = course.SubmittedAt,
        ApprovedAt = course.ApprovedAt,
        ReviewNote = course.ReviewNote
    };

    // Npgsql 6+ requires DateTime values to have an explicit Kind when targeting
    // 'timestamp with time zone'. JSON deserialization leaves Kind=Unspecified,
    // so we normalise to UTC here instead of everywhere at the call site.
    private static DateTime? ToUtc(DateTime? d)
        => d.HasValue ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null;
}
