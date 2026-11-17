using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Shared.Dtos;

public sealed class SetTaskTagsRequest
{
    [Required]
    public List<int> TagIds { get; set; } = [];
}