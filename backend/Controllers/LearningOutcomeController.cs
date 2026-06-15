using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;

namespace Backend.Controllers;

[ApiController]
[Route("api/courses/{courseId}/learning-outcomes")]
[Authorize]
public class LearningOutcomeController : ControllerBase
{
    private readonly AppDbContext _context;
    public LearningOutcomeController(AppDbContext context) => _context = context;

    private int GetUserId() =>
        int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

    private async Task<bool> IsOwner(int courseId) =>
        await _context.Courses.AnyAsync(c => c.Id == courseId && c.InstructorId == GetUserId() && !c.IsLocked);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LearningOutcomeDto>>> GetOutcomes(int courseId)
    {
        var outcomes = await _context.LearningOutcomes
            .Where(lo => lo.CourseId == courseId)
            .OrderBy(lo => lo.Code)
            .Select(lo => new LearningOutcomeDto { Id = lo.Id, Code = lo.Code, Description = lo.Description, BloomLevel = lo.BloomLevel, Component = lo.Component })
            .ToListAsync();
        return Ok(outcomes);
    }

    [HttpPost]
    public async Task<ActionResult<LearningOutcomeDto>> AddOutcome(int courseId, SaveLearningOutcomeRequest request)
    {
        if (!await IsOwner(courseId)) return Forbid();

        var count = await _context.LearningOutcomes.CountAsync(lo => lo.CourseId == courseId);
        var lo = new LearningOutcome
        {
            CourseId = courseId,
            Code = $"ÖÇ{count + 1}",
            Description = request.Description,
            BloomLevel = request.BloomLevel,
            Component = request.Component
        };
        _context.LearningOutcomes.Add(lo);
        await _context.SaveChangesAsync();
        return Ok(new LearningOutcomeDto { Id = lo.Id, Code = lo.Code, Description = lo.Description, BloomLevel = lo.BloomLevel, Component = lo.Component });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOutcome(int courseId, int id, SaveLearningOutcomeRequest request)
    {
        if (!await IsOwner(courseId)) return Forbid();

        var lo = await _context.LearningOutcomes.FirstOrDefaultAsync(lo => lo.Id == id && lo.CourseId == courseId);
        if (lo == null) return NotFound();

        lo.Description = request.Description;
        lo.BloomLevel = request.BloomLevel;
        lo.Component = request.Component;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOutcome(int courseId, int id)
    {
        if (!await IsOwner(courseId)) return Forbid();

        var lo = await _context.LearningOutcomes.FirstOrDefaultAsync(lo => lo.Id == id && lo.CourseId == courseId);
        if (lo == null) return NotFound();

        _context.LearningOutcomes.Remove(lo);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
