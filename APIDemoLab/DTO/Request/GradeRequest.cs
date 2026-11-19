using System.ComponentModel.DataAnnotations;

namespace APIDemoLab.DTO.Request;

public sealed record GradeRequest(
    [Required, StringLength(45)] string Name,
    [Required, StringLength(45)] string Description);
