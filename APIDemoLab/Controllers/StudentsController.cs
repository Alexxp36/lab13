using APIDemoLab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIDemoLab.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StudentsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<Student>> List(string? firstName, string? lastName, string? email,
        CancellationToken cancellationToken)
    {
        var students = db.Students.AsNoTracking().Include(student => student.Grade);
        return await students
            .Where(student => (firstName == null || student.FirstName.Contains(firstName))
                && (lastName == null || student.LastName.Contains(lastName))
                && (email == null || student.Email.Contains(email)))
            .OrderByDescending(student => student.LastName)
            .ThenBy(student => student.StudentId)
            .ToListAsync(cancellationToken);
    }

    [HttpGet("by-grade")]
    public async Task<List<Student>> ByGrade(string? firstName, string? grade, CancellationToken cancellationToken)
    {
        return await db.Students.AsNoTracking().Include(student => student.Grade)
            .Where(student => (firstName == null || student.FirstName.Contains(firstName))
                && (grade == null || student.Grade.Name.Contains(grade)))
            .OrderByDescending(student => db.Enrollments
                .Where(enrollment => enrollment.StudentId == student.StudentId)
                .Select(enrollment => enrollment.Course.Name)
                .OrderByDescending(name => name)
                .FirstOrDefault())
            .ThenBy(student => student.StudentId)
            .ToListAsync(cancellationToken);
    }
}
