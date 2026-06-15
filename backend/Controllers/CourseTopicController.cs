using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;

namespace Backend.Controllers;

[ApiController]
[Route("api/courses/{courseId}/topics")]
[Authorize]
public class CourseTopicController : ControllerBase
{
    private readonly AppDbContext _context;
    public CourseTopicController(AppDbContext context) => _context = context;

    private int GetUserId() =>
        int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

    private async Task<bool> IsOwner(int courseId) =>
        await _context.Courses.AnyAsync(c => c.Id == courseId && c.InstructorId == GetUserId() && !c.IsLocked);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseTopicDto>>> GetTopics(int courseId)
    {
        var topics = await _context.CourseTopics
            .Where(t => t.CourseId == courseId)
            .OrderBy(t => t.OrderNumber)
            .Select(t => new CourseTopicDto { Id = t.Id, OrderNumber = t.OrderNumber, Title = t.Title, Description = t.Description })
            .ToListAsync();
        return Ok(topics);
    }

    [HttpPost]
    public async Task<ActionResult<CourseTopicDto>> AddTopic(int courseId, SaveCourseTopicRequest request)
    {
        if (!await IsOwner(courseId)) return Forbid();

        var topic = new CourseTopic
        {
            CourseId = courseId,
            OrderNumber = request.OrderNumber,
            Title = request.Title,
            Description = request.Description
        };
        _context.CourseTopics.Add(topic);
        await _context.SaveChangesAsync();
        return Ok(new CourseTopicDto { Id = topic.Id, OrderNumber = topic.OrderNumber, Title = topic.Title, Description = topic.Description });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTopic(int courseId, int id, SaveCourseTopicRequest request)
    {
        if (!await IsOwner(courseId)) return Forbid();

        var topic = await _context.CourseTopics.FirstOrDefaultAsync(t => t.Id == id && t.CourseId == courseId);
        if (topic == null) return NotFound();

        topic.OrderNumber = request.OrderNumber;
        topic.Title = request.Title;
        topic.Description = request.Description;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTopic(int courseId, int id)
    {
        if (!await IsOwner(courseId)) return Forbid();

        var topic = await _context.CourseTopics.FirstOrDefaultAsync(t => t.Id == id && t.CourseId == courseId);
        if (topic == null) return NotFound();

        _context.CourseTopics.Remove(topic);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
