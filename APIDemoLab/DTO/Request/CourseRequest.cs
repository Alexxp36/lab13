using System.ComponentModel.DataAnnotations;

namespace APIDemoLab.DTO.Request;

public sealed record CourseRequest(
    [Required, StringLength(45)] string Name,
    [Range(0, int.MaxValue)] int Credit);
