using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Abstractions;

public sealed record TaskFilter(
    TaskItemStatus? Status = null,
    TaskPriority? Priority = null,
    int? ProjectId = null,
    string? Search = null);
