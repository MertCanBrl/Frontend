using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;

namespace Backend.Controllers;

[ApiController]
[Route("api/courses/{courseId}/mapping")]
[Authorize]
public class LOPOMappingController : ControllerBase
{
    private readonly AppDbContext _context;
    public LOPOMappingController(AppDbContext context) => _context = context;

    private int GetUserId() =>
        int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

    // GET: api/courses/{courseId}/mapping
    [HttpGet]
    public async Task<ActionResult<MappingMatrixDto>> GetMatrix(int courseId)
    {
        var learningOutcomes = await _context.LearningOutcomes
            .Where(lo => lo.CourseId == courseId)
            .OrderBy(lo => lo.Code)
            .Select(lo => new LearningOutcomeDto { Id = lo.Id, Code = lo.Code, Description = lo.Description })
            .ToListAsync();

        var programOutcomes = await _context.ProgramOutcomes
            .OrderBy(po => po.Code)
            .Select(po => new ProgramOutcomeDto { Id = po.Id, Code = po.Code, Description = po.Description })
            .ToListAsync();

        var loIds = learningOutcomes.Select(lo => lo.Id).ToList();
        var mappings = await _context.LOPOMappings
            .Where(m => loIds.Contains(m.LearningOutcomeId))
            .Select(m => new MappingCellDto
            {
                LearningOutcomeId = m.LearningOutcomeId,
                ProgramOutcomeId = m.ProgramOutcomeId,
                ContributionLevel = m.ContributionLevel
            })
            .ToListAsync();

        return Ok(new MappingMatrixDto
        {
            LearningOutcomes = learningOutcomes,
            ProgramOutcomes = programOutcomes,
            Mappings = mappings
        });
    }

    // PUT: api/courses/{courseId}/mapping/cell
    [HttpPut("cell")]
    public async Task<IActionResult> UpdateCell(int courseId, MappingCellDto request)
    {
        var isOwner = await _context.Courses.AnyAsync(c => c.Id == courseId && c.InstructorId == GetUserId() && !c.IsLocked);
        if (!isOwner) return Forbid();

        var loExists = await _context.LearningOutcomes.AnyAsync(lo => lo.Id == request.LearningOutcomeId && lo.CourseId == courseId);
        if (!loExists) return BadRequest("Öğrenme çıktısı bu derse ait değil.");

        var existing = await _context.LOPOMappings.FirstOrDefaultAsync(m =>
            m.LearningOutcomeId == request.LearningOutcomeId &&
            m.ProgramOutcomeId == request.ProgramOutcomeId);

        if (existing != null)
        {
            existing.ContributionLevel = request.ContributionLevel;
        }
        else
        {
            _context.LOPOMappings.Add(new LOPOMapping
            {
                LearningOutcomeId = request.LearningOutcomeId,
                ProgramOutcomeId = request.ProgramOutcomeId,
                ContributionLevel = request.ContributionLevel
            });
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
