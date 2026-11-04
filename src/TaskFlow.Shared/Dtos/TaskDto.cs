using TaskFlow.Domain.Enums;

namespace TaskFlow.Shared.Dtos;

public sealed record TaskDto(
    int Id,
    string Title,
    string? Description,
    TaskPriority Priority,
    TaskItemStatus Status,
    DateTime? DueDate,
    DateTime? CompletedAt,
    int? ProjectId,
    DateTime CreatedAt,
    bool IsOverdue);
