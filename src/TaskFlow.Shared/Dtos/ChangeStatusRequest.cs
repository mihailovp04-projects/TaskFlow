using System.ComponentModel.DataAnnotations;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Shared.Dtos;

public sealed class ChangeStatusRequest
{
    [Required]
    [EnumDataType(typeof(TaskItemStatus))]
    public TaskItemStatus? Status { get; set; }
}
