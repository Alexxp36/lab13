using System.ComponentModel.DataAnnotations.Schema;

namespace APIDemoLab.Models;

[Table("Enrollment")]
public sealed class Enrollment
{
    [Column("idEnrollment")]
    public int EnrollmentId { get; set; }

    [Column("Student_idStudent")]
    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    [Column("Course_idCourse")]
    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime Date { get; set; }
}
