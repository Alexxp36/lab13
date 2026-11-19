using APIDemoLab.DTO.Response;
using APIDemoLab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIDemoLab.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EnrollmentsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("by-course")]
    public Task<List<EnrollmentResponse>> ByCourse(string? course, CancellationToken cancellationToken) =>
        List(db.Enrollments.Where(enrollment => course == null || enrollment.Course.Name.Contains(course)),
            cancellationToken);

    [HttpGet("by-grade")]
    public Task<List<EnrollmentResponse>> ByGrade(string? grade, CancellationToken cancellationToken) =>
        List(db.Enrollments.Where(enrollment => grade == null || enrollment.Student.Grade.Name.Contains(grade)),
            cancellationToken);

    private static Task<List<EnrollmentResponse>> List(IQueryable<Enrollment> enrollments,
        CancellationToken cancellationToken) =>
        enrollments.AsNoTracking().Include(enrollment => enrollment.Student.Grade)
            .OrderBy(enrollment => enrollment.Course.Name)
            .ThenBy(enrollment => enrollment.Student.LastName)
            .ThenBy(enrollment => enrollment.EnrollmentId)
            .Select(enrollment => new EnrollmentResponse(enrollment.Student, enrollment.Course,
                new EnrollmentDetails(enrollment.EnrollmentId, enrollment.StudentId,
                    enrollment.CourseId, enrollment.Date)))
            .ToListAsync(cancellationToken);
}
