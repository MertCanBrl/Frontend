using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;

namespace Backend.Controllers;

[ApiController]
[Route("api/program-outcomes")]
[Authorize]
public class ProgramOutcomeController : ControllerBase
{
    private readonly AppDbContext _context;
    public ProgramOutcomeController(AppDbContext context) => _context = context;

    // GET: api/program-outcomes  (tüm roller görebilir)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProgramOutcomeDto>>> GetAll()
    {
        var outcomes = await _context.ProgramOutcomes
            .Include(po => po.Group)
            .OrderBy(po => po.Code)
            .Select(po => new ProgramOutcomeDto
            {
                Id = po.Id,
                Code = po.Code,
                Description = po.Description,
                Details = po.Details,
                GroupId = po.GroupId,
                GroupName = po.Group != null ? po.Group.Name : null
            })
            .ToListAsync();
        return Ok(outcomes);
    }

    // GET: api/program-outcomes/groups
    [HttpGet("groups")]
    public async Task<ActionResult<IEnumerable<ProgramOutcomeGroupDto>>> GetGroups()
    {
        var groups = await _context.ProgramOutcomeGroups
            .Include(g => g.ProgramOutcomes)
            .OrderBy(g => g.Code)
            .Select(g => new ProgramOutcomeGroupDto
            {
                Id = g.Id,
                Code = g.Code,
                Name = g.Name,
                ProgramOutcomes = g.ProgramOutcomes
                    .OrderBy(po => po.Code)
                    .Select(po => new ProgramOutcomeDto { Id = po.Id, Code = po.Code, Description = po.Description })
                    .ToList()
            })
            .ToListAsync();
        return Ok(groups);
    }

    // POST: api/program-outcomes  (sadece Admin)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProgramOutcomeDto>> Create(SaveProgramOutcomeRequest request)
    {
        var po = new ProgramOutcome
        {
            Code = request.Code,
            Description = request.Description,
            Details = request.Details,
            GroupId = request.GroupId
        };
        _context.ProgramOutcomes.Add(po);
        await _context.SaveChangesAsync();

        await _context.Entry(po).Reference(p => p.Group).LoadAsync();

        return Ok(new ProgramOutcomeDto
        {
            Id = po.Id,
            Code = po.Code,
            Description = po.Description,
            Details = po.Details,
            GroupId = po.GroupId,
            GroupName = po.Group?.Name
        });
    }

    // DELETE: api/program-outcomes/{id}  (sadece Admin)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var po = await _context.ProgramOutcomes.FindAsync(id);
        if (po == null) return NotFound();
        _context.ProgramOutcomes.Remove(po);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
