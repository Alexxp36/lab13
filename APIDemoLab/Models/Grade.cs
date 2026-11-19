using System.ComponentModel.DataAnnotations.Schema;

namespace APIDemoLab.Models;

[Table("Grade")]
public sealed class Grade
{
    [Column("idGrade")]
    public int GradeId { get; set; }

    [Column(TypeName = "varchar(45)")]
    public required string Name { get; set; }

    [Column(TypeName = "varchar(45)")]
    public required string Description { get; set; }

    public bool IsActive { get; set; } = true;
}
