using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Shared.Dtos;

public sealed class CreateTagRequest
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
}
