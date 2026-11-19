using APIDemoLab.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace APIDemoLab.Tests;

public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly string connectionString = new SqlConnectionStringBuilder
    {
        DataSource = Environment.GetEnvironmentVariable("LAB13_TEST_SQL_SERVER") ?? @".\SQLEXPRESS01",
        InitialCatalog = $"Lab13Tests_{Guid.NewGuid():N}",
        IntegratedSecurity = true,
        TrustServerCertificate = true
    }.ConnectionString;

    private WebApplicationFactory<Program>? factory;
    public HttpClient Client { get; private set; } = null!;

    public AsyncServiceScope OpenScope() => factory!.Services.CreateAsyncScope();

    public async Task InitializeAsync()
    {
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
            builder.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
        });
        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        await using var scope = OpenScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var activeConnection = new SqlConnectionStringBuilder(db.Database.GetConnectionString());
        if (!activeConnection.EquivalentTo(new SqlConnectionStringBuilder(connectionString)))
        {
            throw new InvalidOperationException("El contexto de pruebas no apunta a la base temporal.");
        }

        await db.Database.MigrateAsync();

        var primero = new Grade { Name = "Primero", Description = "Primer grado" };
        var segundo = new Grade { Name = "Segundo", Description = "Segundo grado" };
        var algebra = new Course { Name = "Álgebra", Credit = 3 };
        var zoologia = new Course { Name = "Zoología", Credit = 4 };
        var perez = Student("Ana", "Pérez", "ana.perez@example.test", primero);
        var vega = Student("Ana", "Vega", "ana.vega@example.test", primero);
        var rojas = Student("Ana", "Rojas", "ana.rojas@example.test", primero);
        var soto = Student("Luis", "Soto", "luis.soto@example.test", segundo);

        db.AddRange(rojas, soto);
        db.Enrollments.AddRange(Enrollment(perez, zoologia), Enrollment(vega, algebra),
            Enrollment(perez, algebra), Enrollment(soto, zoologia));
        await db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (factory is not null)
        {
            await factory.DisposeAsync();
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString).Options;
        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureDeletedAsync();
    }

    private static Student Student(string firstName, string lastName, string email, Grade grade) =>
        new() { FirstName = firstName, LastName = lastName, Email = email, Phone = "123456789", Grade = grade };

    private static Enrollment Enrollment(Student student, Course course) =>
        new() { Student = student, Course = course, Date = new DateTime(2026, 1, 15) };
}
