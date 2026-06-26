using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;
using Backend.Services;
using Backend.Utils;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public AdminController(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // GET: api/admin/users
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _context.Users
            .OrderBy(u => u.FullName)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role
            })
            .ToListAsync();

        return Ok(users);
    }

    // POST: api/admin/users
    [HttpPost("users")]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequest request)
    {
        var exists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
            return Conflict("Bu e-posta adresi zaten kullanımda.");

        var password = GeneratePassword();
        var displayName = string.IsNullOrWhiteSpace(request.Title)
            ? request.FullName
            : $"{request.Title} {request.FullName}";

        var user = new User
        {
            FullName = displayName,
            Email = request.Email,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = request.IsAdmin ? "Admin" : "Instructor"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var adminName = User.FindFirst("fullName")?.Value ?? "Bölüm Başkanı";
        var adminEmail = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? "";
        await _emailService.SendCredentialsAsync(user.Email, user.FullName, password, adminName, adminEmail);

        return Ok(new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role
        });
    }

    // PUT: api/admin/users/{id}
    [HttpPut("users/{id:int}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, UpdateUserRequest request)
    {
        // Not: [ApiController] sayesinde model doğrulama hataları (zorunlu alan, e-posta,
        // telefon formatı) bu noktaya gelmeden 400 ValidationProblemDetails olarak döner.
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Güncellenmek istenen kullanıcı bulunamadı." });

        // E-posta başka bir kullanıcıda kullanılıyor mu?
        var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != id);
        if (emailExists)
            return Conflict(new { message = "Bu e-posta adresi zaten başka bir kullanıcıda kullanılıyor." });

        // Güvenlik kuralı: Yönetici kendi admin yetkisini kaldıramaz (sistemin admin'siz
        // kalmasını ve yanlışlıkla yetki kaybını önler).
        if (id == GetUserId() && user.Role == "Admin" && !request.IsAdmin)
            return Conflict(new { message = "Kendi yönetici (admin) yetkinizi kaldıramazsınız." });

        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        user.Role = request.IsAdmin ? "Admin" : "Instructor";

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"UpdateUser error (id={id}): {ex.Message}\n{ex.InnerException?.Message}");
            return StatusCode(500, new
            {
                message = "Kullanıcı güncellenirken bir veritabanı hatası oluştu.",
                detail = ex.InnerException?.Message ?? ex.Message
            });
        }

        return Ok(new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role
        });
    }

    // DELETE: api/admin/users/{id}
    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Silinmek istenen kullanıcı bulunamadı." });

        // Kullanıcı kendi hesabını silemez.
        if (id == GetUserId())
            return Conflict(new { message = "Kendi hesabınızı silemezsiniz." });

        // Güvenli silme: atanmış dersi olan öğretim üyesi silinemez (önce ders ataması kaldırılmalı).
        var hasCourses = await _context.Courses.AnyAsync(c => c.InstructorId == id);
        if (hasCourses)
            return Conflict(new { message = "Bu kullanıcının atanmış dersleri bulunduğu için silinemez. Önce dersleri başka bir öğretim üyesine atayın." });

        _context.Users.Remove(user);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"DeleteUser error (id={id}): {ex.Message}\n{ex.InnerException?.Message}");
            return Conflict(new { message = "Bu kullanıcı başka kayıtlarla ilişkili olduğu için silinemedi." });
        }

        return NoContent();
    }

    // GET: api/admin/users/{id}/courses
    [HttpGet("users/{id:int}/courses")]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetUserCourses(int id)
    {
        if (!await _context.Users.AnyAsync(u => u.Id == id))
            return NotFound(new { message = "Kullanıcı bulunamadı." });

        var courses = await _context.Courses
            .Where(c => c.InstructorId == id)
            .OrderBy(c => c.Code)
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Semester = c.Semester,
                Credit = c.Credit,
                IsMandatory = c.IsMandatory,
                ClassYear = c.ClassYear,
                InstructorId = c.InstructorId,
                InstructorName = null
            })
            .ToListAsync();

        return Ok(courses);
    }

    // GET: api/admin/courses
    [HttpGet("courses")]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
    {
        var courses = await _context.Courses
            .Include(c => c.Instructor)
            .OrderBy(c => c.Code)
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Semester = c.Semester,
                Credit = c.Credit,
                IsMandatory = c.IsMandatory,
                ClassYear = c.ClassYear,
                InstructorId = c.InstructorId,
                InstructorName = c.Instructor != null ? c.Instructor.FullName : null
            })
            .ToListAsync();

        return Ok(courses);
    }

    // POST: api/admin/courses
    [HttpPost("courses")]
    public async Task<ActionResult<CourseDto>> CreateCourse(CreateCourseRequest request)
    {
        var codeExists = await _context.Courses.AnyAsync(c => c.Code == request.Code);
        if (codeExists)
            return Conflict("Bu ders kodu zaten kullanımda.");

        if (request.ClassYear < 1 || request.ClassYear > 4)
            return BadRequest("Sınıf 1 ile 4 arasında olmalıdır.");

        var course = new Course
        {
            Code = request.Code,
            Name = request.Name,
            Semester = request.Semester,
            Credit = request.Credit,
            IsMandatory = request.IsMandatory,
            ClassYear = request.ClassYear,
            InstructorId = request.InstructorId
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        await _context.Entry(course).Reference(c => c.Instructor).LoadAsync();

        return Ok(new CourseDto
        {
            Id = course.Id,
            Code = course.Code,
            Name = course.Name,
            Semester = course.Semester,
            Credit = course.Credit,
            IsMandatory = course.IsMandatory,
            ClassYear = course.ClassYear,
            InstructorId = course.InstructorId,
            InstructorName = course.Instructor?.FullName
        });
    }

    // PUT: api/admin/courses/{id}
    [HttpPut("courses/{id:int}")]
    public async Task<ActionResult<CourseDto>> UpdateCourse(int id, AdminUpdateCourseRequest request)
    {
        // Not: [ApiController] sayesinde model doğrulama hataları (zorunlu alan, Range,
        // MaxLength) bu noktaya gelmeden 400 ValidationProblemDetails olarak döner.
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
            return NotFound(new { message = "Güncellenmek istenen ders bulunamadı." });

        // Ders kodu başka bir derste kullanılıyor mu?
        var codeExists = await _context.Courses.AnyAsync(c => c.Code == request.Code && c.Id != id);
        if (codeExists)
            return Conflict(new { message = "Bu ders kodu zaten başka bir derste kullanılıyor." });

        // Öğretim üyesi atandıysa gerçekten var mı?
        if (request.InstructorId.HasValue &&
            !await _context.Users.AnyAsync(u => u.Id == request.InstructorId && u.Role == "Instructor"))
            return BadRequest(new { message = "Seçilen öğretim üyesi bulunamadı." });

        course.Code = request.Code;
        course.Name = request.Name;
        course.Semester = request.Semester;
        course.Credit = request.Credit;
        course.IsMandatory = request.IsMandatory;
        course.ClassYear = request.ClassYear;
        course.InstructorId = request.InstructorId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"UpdateCourse error (id={id}): {ex.Message}\n{ex.InnerException?.Message}");
            return StatusCode(500, new
            {
                message = "Ders güncellenirken bir veritabanı hatası oluştu.",
                detail = ex.InnerException?.Message ?? ex.Message
            });
        }

        await _context.Entry(course).Reference(c => c.Instructor).LoadAsync();

        return Ok(new CourseDto
        {
            Id = course.Id,
            Code = course.Code,
            Name = course.Name,
            Semester = course.Semester,
            Credit = course.Credit,
            IsMandatory = course.IsMandatory,
            ClassYear = course.ClassYear,
            InstructorId = course.InstructorId,
            InstructorName = course.Instructor?.FullName
        });
    }

    // DELETE: api/admin/courses/{id}
    [HttpDelete("courses/{id:int}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
            return NotFound(new { message = "Silinmek istenen ders bulunamadı." });

        // Güvenli silme: derse kayıtlı öğrenci/not verisi varsa silmeyi engelle.
        var hasEnrollments = await _context.Enrollments.AnyAsync(e => e.CourseId == id);
        if (hasEnrollments)
            return Conflict(new { message = "Bu derse kayıtlı öğrenciler bulunduğu için ders silinemez." });

        _context.Courses.Remove(course);

        try
        {
            // Derse bağlı içerik (konular, öğrenme çıktıları, sınavlar vb.) cascade ile silinir.
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"DeleteCourse error (id={id}): {ex.Message}\n{ex.InnerException?.Message}");
            return Conflict(new { message = "Bu ders başka kayıtlarla ilişkili olduğu için silinemedi." });
        }

        return NoContent();
    }

    // ── Onay Yönetimi ────────────────────────────────────────────────────────

    // GET: api/admin/approvals
    [HttpGet("approvals")]
    public async Task<ActionResult<IEnumerable<ApprovalListItemDto>>> GetApprovals()
    {
        var items = await _context.Courses
            .Include(c => c.Instructor)
            .Where(c => c.ContentStatus != "Draft")
            .OrderByDescending(c => c.SubmittedAt)
            .Select(c => new ApprovalListItemDto
            {
                CourseId = c.Id,
                CourseCode = c.Code,
                CourseName = c.Name,
                InstructorName = c.Instructor != null ? c.Instructor.FullName : null,
                SubmittedAt = c.SubmittedAt,
                ContentStatus = c.ContentStatus
            })
            .ToListAsync();

        return Ok(items);
    }

    // POST: api/admin/approvals/{courseId}/approve
    [HttpPost("approvals/{courseId:int}/approve")]
    public async Task<IActionResult> ApproveCourse(int courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return NotFound();

        if (course.ContentStatus != "PendingApproval")
            return Conflict("Bu ders onay bekliyor durumunda değil.");

        course.ContentStatus = "Approved";
        course.IsLocked = true;
        course.ApprovedAt = DateTime.UtcNow;
        course.ReviewedByUserId = GetUserId();
        course.ReviewNote = null;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // POST: api/admin/approvals/{courseId}/request-revision
    [HttpPost("approvals/{courseId:int}/request-revision")]
    public async Task<IActionResult> RequestRevision(int courseId, RequestRevisionRequest request)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return NotFound();

        if (course.ContentStatus != "PendingApproval" && course.ContentStatus != "Approved")
            return Conflict("Revizyon yalnızca onay bekleyen veya onaylanmış dersler için istenebilir.");

        course.ContentStatus = "RevisionRequested";
        course.IsLocked = false;
        course.ReviewNote = request.Note;
        course.ReviewedByUserId = GetUserId();
        course.ApprovedAt = null;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ── Genel Anket Soruları Yönetimi ────────────────────────────────────────

    [HttpGet("general-survey-questions")]
    public async Task<ActionResult<IEnumerable<GeneralSurveyQuestionDto>>> GetGeneralSurveyQuestions()
    {
        var questions = await _context.GeneralSurveyQuestions
            .OrderBy(q => q.OrderNumber)
            .Select(q => new GeneralSurveyQuestionDto { Id = q.Id, QuestionText = q.QuestionText, OrderNumber = q.OrderNumber })
            .ToListAsync();
        return Ok(questions);
    }

    [HttpPost("general-survey-questions")]
    public async Task<ActionResult<GeneralSurveyQuestionDto>> AddGeneralSurveyQuestion([FromBody] SaveGeneralSurveyQuestionRequest request)
    {
        var maxOrder = await _context.GeneralSurveyQuestions.AnyAsync()
            ? await _context.GeneralSurveyQuestions.MaxAsync(q => q.OrderNumber)
            : 0;

        var question = new GeneralSurveyQuestion
        {
            QuestionText = request.QuestionText,
            OrderNumber = maxOrder + 1,
            IsActive = true
        };
        _context.GeneralSurveyQuestions.Add(question);
        await _context.SaveChangesAsync();
        return Ok(new GeneralSurveyQuestionDto { Id = question.Id, QuestionText = question.QuestionText, OrderNumber = question.OrderNumber });
    }

    [HttpPut("general-survey-questions/{id:int}")]
    public async Task<IActionResult> UpdateGeneralSurveyQuestion(int id, [FromBody] SaveGeneralSurveyQuestionRequest request)
    {
        var question = await _context.GeneralSurveyQuestions.FindAsync(id);
        if (question == null) return NotFound();
        question.QuestionText = request.QuestionText;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("general-survey-questions/{id:int}")]
    public async Task<IActionResult> DeleteGeneralSurveyQuestion(int id)
    {
        var question = await _context.GeneralSurveyQuestions.FindAsync(id);
        if (question == null) return NotFound();
        _context.GeneralSurveyQuestions.Remove(question);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ── Onay — Ders İçeriği Önizleme ─────────────────────────────────────────

    // GET: api/admin/courses/{courseId}/content
    [HttpGet("courses/{courseId:int}/content")]
    public async Task<ActionResult<AdminCourseContentDto>> GetCourseContent(int courseId)
    {
        var data = await LoadCourseContentAsync(courseId);
        if (data == null) return NotFound();
        return Ok(data);
    }

    // GET: api/admin/courses/{courseId}/pdf
    [HttpGet("courses/{courseId:int}/pdf")]
    public async Task<IActionResult> GetCoursePdf(int courseId)
    {
        var data = await LoadCourseContentAsync(courseId);
        if (data == null) return NotFound();

        var pdfBytes = CoursePdfService.Generate(data);
        var fileName = $"ders-icerigi-{data.CourseDetail.Code.Replace("/", "-")}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    private async Task<AdminCourseContentDto?> LoadCourseContentAsync(int courseId)
    {
        var course = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.CourseTopics)
            .Include(c => c.LearningOutcomes)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null) return null;

        var loIds = course.LearningOutcomes.Select(lo => lo.Id).ToList();

        var mappings = await _context.LOPOMappings
            .Where(m => loIds.Contains(m.LearningOutcomeId))
            .Select(m => new MappingCellDto
            {
                LearningOutcomeId = m.LearningOutcomeId,
                ProgramOutcomeId = m.ProgramOutcomeId,
                ContributionLevel = m.ContributionLevel
            })
            .ToListAsync();

        var programOutcomes = NaturalSortHelper.ByCode(
            await _context.ProgramOutcomes
                .Select(po => new ProgramOutcomeDto { Id = po.Id, Code = po.Code, Description = po.Description })
                .ToListAsync(),
            po => po.Code).ToList();

        var surveyQuestions = await _context.CourseSurveyQuestions
            .Include(q => q.LOWeights)
            .ThenInclude(w => w.LearningOutcome)
            .Where(q => q.CourseId == courseId)
            .Select(q => new SurveyQuestionDto
            {
                Id = q.Id,
                CourseId = q.CourseId,
                QuestionText = q.QuestionText,
                IsActive = q.IsActive,
                LOWeights = q.LOWeights.Select(w => new SurveyQuestionLOWeightDto
                {
                    LearningOutcomeId = w.LearningOutcomeId,
                    LearningOutcomeCode = w.LearningOutcome.Code,
                    WeightPercentage = w.WeightPercentage
                }).ToList()
            })
            .ToListAsync();

        var learningOutcomes = NaturalSortHelper.ByCode(
            course.LearningOutcomes
                .Select(lo => new LearningOutcomeDto
                {
                    Id = lo.Id,
                    Code = lo.Code,
                    Description = lo.Description,
                    BloomLevel = lo.BloomLevel,
                    Component = lo.Component
                })
                .ToList(),
            lo => lo.Code).ToList();

        var topics = course.CourseTopics
            .OrderBy(t => t.OrderNumber)
            .Select(t => new CourseTopicDto
            {
                Id = t.Id,
                OrderNumber = t.OrderNumber,
                Title = t.Title,
                Description = t.Description
            })
            .ToList();

        return new AdminCourseContentDto
        {
            CourseDetail = MapToCourseDetailDto(course),
            Topics = topics,
            LearningOutcomes = learningOutcomes,
            Matrix = new MappingMatrixDto
            {
                LearningOutcomes = learningOutcomes,
                ProgramOutcomes = programOutcomes,
                Mappings = mappings
            },
            SurveyQuestions = surveyQuestions
        };
    }

    // ── Yardımcı metotlar ────────────────────────────────────────────────────

    private int GetUserId()
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null) throw new InvalidOperationException("UserId claim bulunamadı.");
        return int.Parse(claim.Value);
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

    private static string GeneratePassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#";
        return new string(Enumerable.Range(0, 12)
            .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])
            .ToArray());
    }
}
