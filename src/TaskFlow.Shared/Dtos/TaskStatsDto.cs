namespace TaskFlow.Shared.Dtos;

public sealed record TaskStatsDto(
    int Total,
    IReadOnlyDictionary<string, int> ByStatus,
    IReadOnlyDictionary<string, int> ByPriority,
    int Overdue);