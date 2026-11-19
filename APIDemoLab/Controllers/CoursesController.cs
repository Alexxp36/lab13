using APIDemoLab.DTO.Request;
using APIDemoLab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIDemoLab.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CoursesController(ApplicationDbContext db) : ControllerBase
{
    private IQueryable<Course> Active => db.Courses.Where(item => item.IsActive);

    [HttpGet]
    public async Task<List<Course>> List(CancellationToken cancellationToken) =>
        await Active.AsNoTracking().OrderBy(item => item.CourseId).ToListAsync(cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Course>> Get(int id, CancellationToken cancellationToken)
    {
        var item = await Active.AsNoTracking()
            .SingleOrDefaultAsync(item => item.CourseId == id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Course>> Create(CourseRequest request, CancellationToken cancellationToken)
    {
        var item = new Course
        {
            Name = request.Name,
            Credit = request.Credit
        };
        db.Courses.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = item.CourseId }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CourseRequest request, CancellationToken cancellationToken)
    {
        var affected = await Active.Where(item => item.CourseId == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.Name, request.Name)
                .SetProperty(item => item.Credit, request.Credit), cancellationToken);
        return affected == 0 ? NotFound() : NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var affected = await Active.Where(item => item.CourseId == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.IsActive, false), cancellationToken);
        return affected == 0 ? NotFound() : NoContent();
    }
}
