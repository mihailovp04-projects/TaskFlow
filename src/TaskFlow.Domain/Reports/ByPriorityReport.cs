using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Reports;

public sealed class ByPriorityReport : TaskReport
{
    public override string Title => "Задачи по приоритету";

    protected override IReadOnlyList<string> Generate(IEnumerable<TaskItem> tasks, DateTime now) =>
        tasks
            .GroupBy(t => t.Priority)
            .OrderByDescending(g => g.Key)
            .Select(g => $"{g.Key}: всего {g.Count()}, выполнено {g.Count(t => t.Status == TaskItemStatus.Done)}")
            .ToList();
}
