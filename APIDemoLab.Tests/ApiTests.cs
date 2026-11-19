using APIDemoLab.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace APIDemoLab.Tests;

public sealed class ApiTests(SqlServerFixture fixture) : IClassFixture<SqlServerFixture>
{
    [Fact(DisplayName = "05 · SQL Server aplica la migración y crea las cuatro tablas")]
    public async Task AppliesMigration()
    {
        await using var scope = fixture.OpenScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.StartsWith("Lab13Tests_", db.Database.GetDbConnection().Database);
        Assert.EndsWith("_InitialCreate", Assert.Single(await db.Database.GetAppliedMigrationsAsync()));
        var tables = await db.Database.SqlQueryRaw<string>(
            "SELECT [name] AS [Value] FROM sys.tables").ToListAsync();
        Assert.Equal(new[] { "Course", "Enrollment", "Grade", "Student", "__EFMigrationsHistory" },
            tables.OrderBy(name => name, StringComparer.Ordinal));
    }

    [Fact(DisplayName = "06 · CRUD y eliminación lógica de grados")]
    public Task CrudGrades() => CrudAndSoftDelete("grades", "gradeId");

    [Fact(DisplayName = "07 · CRUD y eliminación lógica de cursos")]
    public Task CrudCourses() => CrudAndSoftDelete("courses", "courseId");

    private async Task CrudAndSoftDelete(string collection, string idProperty)
    {
        var url = $"/api/{collection}";
        var isCourse = collection == "courses";
        object create = isCourse ? new { name = "Prueba CRUD", credit = 2 }
            : new { name = "Prueba CRUD", description = "Inicial" };
        object update = isCourse ? new { name = "Prueba editada", credit = 5 }
            : new { name = "Prueba editada", description = "Editada" };

        using var created = await fixture.Client.PostAsJsonAsync(url, create);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var entity = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = entity.GetProperty(idProperty).GetInt32();
        var itemUrl = $"{url}/{id}";
        var location = Assert.IsType<Uri>(created.Headers.Location);
        Assert.EndsWith(itemUrl, location.ToString(), StringComparison.OrdinalIgnoreCase);
        var fetched = await fixture.Client.GetFromJsonAsync<JsonElement>(location);
        Assert.Equal(id, fetched.GetProperty(idProperty).GetInt32());
        Assert.True(entity.GetProperty("isActive").GetBoolean());
        Assert.Contains(await Read(url), item => item.GetProperty(idProperty).GetInt32() == id);

        using var updated = await fixture.Client.PutAsJsonAsync(itemUrl, update);
        Assert.Equal(HttpStatusCode.NoContent, updated.StatusCode);
        var changed = await fixture.Client.GetFromJsonAsync<JsonElement>(itemUrl);
        Assert.Equal("Prueba editada", changed.GetProperty("name").GetString());
        if (isCourse)
        {
            Assert.Equal(5, changed.GetProperty("credit").GetInt32());
        }
        else
        {
            Assert.Equal("Editada", changed.GetProperty("description").GetString());
        }

        using var deleted = await fixture.Client.DeleteAsync(itemUrl);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        using var missing = await fixture.Client.GetAsync(itemUrl);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.DoesNotContain(await Read(url), item => item.GetProperty(idProperty).GetInt32() == id);

        await using var scope = fixture.OpenScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var active = isCourse
            ? (await db.Courses.SingleAsync(course => course.CourseId == id)).IsActive
            : (await db.Grades.SingleAsync(grade => grade.GradeId == id)).IsActive;
        Assert.False(active);
    }

    [Fact(DisplayName = "08 · Busca por nombre, apellido y correo y ordena por apellido descendente")]
    public async Task SearchesStudents()
    {
        Assert.Equal(new[] { "Vega", "Rojas", "Pérez" },
            (await Read("/api/students?firstName=An&email=ana.")).Select(LastName));
        var student = Assert.Single(await Read("/api/students?firstName=An&lastName=P%C3%A9&email=perez"));
        Assert.Equal("Pérez", LastName(student));
        Assert.Empty(await Read("/api/students?firstName=Ana&lastName=Vega&email=perez"));
    }

    [Fact(DisplayName = "09 · Busca por nombre y grado y ordena por curso descendente")]
    public async Task SearchesStudentsByGrade()
    {
        var students = await Read("/api/students/by-grade?firstName=Ana&grade=Primer");
        Assert.Equal(new[] { "Pérez", "Vega", "Rojas" }, students.Select(LastName));
        Assert.All(students, student =>
        {
            var grade = student.GetProperty("grade");
            Assert.Equal("Primero", grade.GetProperty("name").GetString());
            Assert.Equal("Primer grado", grade.GetProperty("description").GetString());
            Assert.Equal(student.GetProperty("gradeId").GetInt32(), grade.GetProperty("gradeId").GetInt32());
        });
        Assert.Empty(await Read("/api/students/by-grade?firstName=Luis&grade=Primer"));
    }

    [Fact(DisplayName = "10 · Busca matrículas por curso y ordena por curso y apellido")]
    public async Task SearchesEnrollmentsByCourse()
    {
        var items = await Read("/api/enrollments/by-course?course=%C3%81lge");
        Assert.Equal(new[] { "Álgebra/Pérez", "Álgebra/Vega" }, items.Select(CourseAndSurname));
        Assert.All(items, CheckEnrollment);
        Assert.Empty(await Read("/api/enrollments/by-course?course=Inexistente"));
    }

    [Fact(DisplayName = "11 · Busca matrículas por grado y ordena por curso y apellido")]
    public async Task SearchesEnrollmentsByGrade()
    {
        var items = await Read("/api/enrollments/by-grade?grade=Primer");
        Assert.Equal(new[] { "Álgebra/Pérez", "Álgebra/Vega", "Zoología/Pérez" }, items.Select(CourseAndSurname));
        Assert.All(items, CheckEnrollment);
        var secondGrade = Assert.Single(await Read("/api/enrollments/by-grade?grade=Segundo"));
        Assert.Equal("Zoología/Soto", CourseAndSurname(secondGrade));
    }

    private async Task<JsonElement[]> Read(string url) =>
        (await fixture.Client.GetFromJsonAsync<JsonElement[]>(url))!;

    private static string? LastName(JsonElement student) => student.GetProperty("lastName").GetString();

    private static string CourseAndSurname(JsonElement item) =>
        $"{item.GetProperty("course").GetProperty("name").GetString()}/{LastName(item.GetProperty("student"))}";

    private static void CheckEnrollment(JsonElement item)
    {
        var enrollment = item.GetProperty("enrollment");
        Assert.True(enrollment.GetProperty("enrollmentId").GetInt32() > 0);
        Assert.Equal(new DateTime(2026, 1, 15), enrollment.GetProperty("date").GetDateTime());
        Assert.Equal(item.GetProperty("student").GetProperty("studentId").GetInt32(),
            enrollment.GetProperty("studentId").GetInt32());
        Assert.Equal(item.GetProperty("course").GetProperty("courseId").GetInt32(),
            enrollment.GetProperty("courseId").GetInt32());
    }
}
