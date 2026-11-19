using System.ComponentModel.DataAnnotations;
using TaskFlow.Shared.Dtos;

namespace TaskFlow.Web.Models;

public sealed class ProjectFormModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name must be at most 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description must be at most 500 characters.")]
    public string? Description { get; set; }

    public CreateProjectRequest ToCreateRequest() => new()
    {
        Name = Name.Trim(),
        Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim()
    };
}
