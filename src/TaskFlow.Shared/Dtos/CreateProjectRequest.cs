using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Shared.Dtos;

public sealed class CreateProjectRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [RegularExpression("^#[0-9A-Fa-f]{6}$")]
    public string? Color { get; set; }

}
