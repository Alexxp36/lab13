using APIDemoLab.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.Json;
using Xunit;

namespace APIDemoLab.Tests;

public sealed class ModelTests
{
    [Fact(DisplayName = "01 · Los cuatro modelos corresponden al diagrama")]
    public void MapsTablesAndColumns()
    {
        using var db = CreateContext();
        CheckEntity<Course>(db, "Course", "idCourse", "Name:varchar(45)", "Credit:int", "IsActive:bit");
        CheckEntity<Grade>(db, "Grade", "idGrade", "Name:varchar(45)", "Description:varchar(45)", "IsActive:bit");
        CheckEntity<Student>(db, "Student", "idStudent", "Grade_idGrade:int", "FirstName:varchar(45)",
            "LastName:varchar(45)", "Phone:varchar(45)", "Email:varchar(45)");
        CheckEntity<Enrollment>(db, "Enrollment", "idEnrollment", "Student_idStudent:int",
            "Course_idCourse:int", "Date:datetime");
    }

    [Fact(DisplayName = "02 · Las tres llaves foráneas apuntan a las tablas correctas")]
    public void MapsForeignKeys()
    {
        using var db = CreateContext();
        var relationships = db.Model.GetEntityTypes().SelectMany(entity => entity.GetForeignKeys())
            .Select(key => $"{key.DeclaringEntityType.GetTableName()}.{Assert.Single(key.Properties).Name}" +
                $" → {key.PrincipalEntityType.GetTableName()}")
            .OrderBy(value => value, StringComparer.Ordinal);
        Assert.Equal(new[] { "Enrollment.CourseId → Course", "Enrollment.StudentId → Student",
            "Student.GradeId → Grade" }, relationships);
    }

    [Fact(DisplayName = "03 · El contexto expone los cuatro DbSet")]
    public void ExposesSets()
    {
        using var db = CreateContext();
        Assert.Same(db.Set<Course>(), db.Courses);
        Assert.Same(db.Set<Grade>(), db.Grades);
        Assert.Same(db.Set<Student>(), db.Students);
        Assert.Same(db.Set<Enrollment>(), db.Enrollments);
    }

    [Fact(DisplayName = "04 · La configuración usa autenticación de Windows")]
    public void UsesIntegratedSecurity()
    {
        using var json = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "appsettings.laboratorio.json")));
        var connection = new SqlConnectionStringBuilder(json.RootElement
            .GetProperty("ConnectionStrings").GetProperty("DefaultConnection").GetString());
        Assert.True(connection.IntegratedSecurity);
        Assert.Empty(connection.UserID);
        Assert.Empty(connection.Password);
    }

    private static ApplicationDbContext CreateContext() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer().Options);

    private static void CheckEntity<TEntity>(ApplicationDbContext db, string tableName, string keyName,
        params string[] columns)
    {
        var entity = db.Model.FindEntityType(typeof(TEntity))!;
        Assert.Equal(tableName, entity.GetTableName());
        var table = StoreObjectIdentifier.Table(tableName, entity.GetSchema());
        var key = Assert.Single(entity.FindPrimaryKey()!.Properties);
        Assert.Equal(keyName, key.GetColumnName(table));
        Assert.Equal("int", key.GetColumnType());
        var otherColumns = entity.GetProperties().Where(property => property != key)
            .Select(property => $"{property.GetColumnName(table)}:{property.GetColumnType()}");
        Assert.Equal(columns.OrderBy(value => value), otherColumns.OrderBy(value => value));
    }
}
