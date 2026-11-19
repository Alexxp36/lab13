using System.ComponentModel.DataAnnotations.Schema;

namespace APIDemoLab.Models;

[Table("Course")]
public sealed class Course
{
    [Column("idCourse")]
    public int CourseId { get; set; }

    [Column(TypeName = "varchar(45)")]
    public required string Name { get; set; }

    public int Credit { get; set; }

    public bool IsActive { get; set; } = true;
}
