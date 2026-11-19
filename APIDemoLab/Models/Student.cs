using System.ComponentModel.DataAnnotations.Schema;

namespace APIDemoLab.Models;

[Table("Student")]
public sealed class Student
{
    [Column("idStudent")]
    public int StudentId { get; set; }

    [Column("Grade_idGrade")]
    public int GradeId { get; set; }

    public Grade Grade { get; set; } = null!;

    [Column(TypeName = "varchar(45)")]
    public required string FirstName { get; set; }

    [Column(TypeName = "varchar(45)")]
    public required string LastName { get; set; }

    [Column(TypeName = "varchar(45)")]
    public required string Phone { get; set; }

    [Column(TypeName = "varchar(45)")]
    public required string Email { get; set; }
}
