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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Instructor"
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
            Role = user.Role
        });
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

    // ── Onay Yönetimi ────────────────────────────────────────────────────────

    // GET: api/admin/approvals
    [HttpGet("approvals")]
    public async Task<ActionResult<IEnumerable<ApprovalListItemDto>>> GetApprovals()
    {
        var items = await _context.Courses
            .Include(c => c.Instructor)
            .Where(c => c.ContentStatus == "PendingApproval")
            .OrderBy(c => c.SubmittedAt)
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

        if (course.ContentStatus != "PendingApproval")
            return Conflict("Bu ders onay bekliyor durumunda değil.");

        course.ContentStatus = "RevisionRequested";
        course.IsLocked = false;
        course.ReviewNote = request.Note;
        course.ReviewedByUserId = GetUserId();
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
