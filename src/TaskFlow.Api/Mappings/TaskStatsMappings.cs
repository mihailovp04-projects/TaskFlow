using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Dtos;

namespace TaskFlow.Api.Mappings;

public static class TaskStatsMappings
{
    public static TaskStatsDto ToDto(this TaskStats stats) =>
        new(
            stats.Total,
            Enum.GetValues<TaskItemStatus>().ToDictionary(s => s.ToString(), s => stats.ByStatus.GetValueOrDefault(s)),
            Enum.GetValues<TaskPriority>().ToDictionary(p => p.ToString(), p => stats.ByPriority.GetValueOrDefault(p)),
            stats.Overdue);
}