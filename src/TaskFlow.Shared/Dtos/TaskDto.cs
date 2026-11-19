using TaskFlow.Domain.Enums;

namespace TaskFlow.Shared.Dtos;

// Задача в том виде, в котором её возвращает API
public sealed record TaskDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskPriority Priority { get; init; }
    public TaskItemStatus Status { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int? ProjectId { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsOverdue { get; init; }
}
