using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Shared.Dtos;

public sealed class UpdateProjectRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }
}
