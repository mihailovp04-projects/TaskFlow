using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Reports;

public sealed class OverdueReport : TaskReport
{
    public override string Title => "Просроченные задачи";

    protected override IReadOnlyList<string> Generate(IEnumerable<TaskItem> tasks, DateTime now) =>
        tasks
            .Where(t => t.IsOverdue(now))
            .OrderBy(t => t.DueDate)
            .ThenByDescending(t => t.Priority)
            .Select(t => FormatTask(t, now))
            .ToList();

    protected override string FormatTask(TaskItem task, DateTime now)
    {
        int daysLate = (now.Date - task.DueDate!.Value.Date).Days;
        return $"{base.FormatTask(task, now)}, просрочено на {daysLate} дн.";
    }
}
