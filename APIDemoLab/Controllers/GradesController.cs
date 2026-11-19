using APIDemoLab.DTO.Request;
using APIDemoLab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIDemoLab.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class GradesController(ApplicationDbContext db) : ControllerBase
{
    private IQueryable<Grade> Active => db.Grades.Where(item => item.IsActive);

    [HttpGet]
    public async Task<List<Grade>> List(CancellationToken cancellationToken) =>
        await Active.AsNoTracking().OrderBy(item => item.GradeId).ToListAsync(cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Grade>> Get(int id, CancellationToken cancellationToken)
    {
        var item = await Active.AsNoTracking()
            .SingleOrDefaultAsync(item => item.GradeId == id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Grade>> Create(GradeRequest request, CancellationToken cancellationToken)
    {
        var item = new Grade
        {
            Name = request.Name,
            Description = request.Description
        };
        db.Grades.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = item.GradeId }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, GradeRequest request, CancellationToken cancellationToken)
    {
        var affected = await Active.Where(item => item.GradeId == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.Name, request.Name)
                .SetProperty(item => item.Description, request.Description), cancellationToken);
        return affected == 0 ? NotFound() : NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var affected = await Active.Where(item => item.GradeId == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.IsActive, false), cancellationToken);
        return affected == 0 ? NotFound() : NoContent();
    }
}
