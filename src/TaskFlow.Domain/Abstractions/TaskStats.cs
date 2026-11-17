using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Abstractions;

public sealed record TaskStats(
    int Total,
    IReadOnlyDictionary<TaskItemStatus, int> ByStatus,
    IReadOnlyDictionary<TaskPriority, int> ByPriority,
    int Overdue);