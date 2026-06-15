using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models.DTOs;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InstructorController : ControllerBase
{
    private readonly AppDbContext _context;

    public InstructorController(AppDbContext context) => _context = context;

    private int GetUserId() =>
        int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

    // GET: api/instructor/my-courses
    [HttpGet("my-courses")]
    public async Task<ActionResult<IEnumerable<InstructorCourseDto>>> GetMyCourses()
    {
        var userId = GetUserId();

        var courses = await _context.Courses
            .Where(c => c.InstructorId == userId)
            .OrderBy(c => c.Code)
            .Select(c => new InstructorCourseDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Semester = c.Semester,
                Credit = c.Credit,
                Akts = c.Akts,
                IsMandatory = c.IsMandatory,
                Department = c.Department,
                ClassYear = c.ClassYear,
                CourseType = c.CourseType,
                IsLocked = c.IsLocked,
                TopicCount = c.CourseTopics.Count,
                LearningOutcomeCount = c.LearningOutcomes.Count
            })
            .ToListAsync();

        return Ok(courses);
    }

    // GET: api/instructor/courses/{id}
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

        return Ok(new CourseDetailDto
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
            InstructorName = course.Instructor?.FullName
        });
    }

    // PUT: api/instructor/courses/{id}
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
}
