using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Reports;

public sealed class DueSoonReport : TaskReport
{
    public override string Title => "Срок в ближайшие 7 дней";

    protected override IReadOnlyList<string> Generate(IEnumerable<TaskItem> tasks, DateTime now)
    {
        DateTime today = now.Date;
        DateTime limit = today.AddDays(7);

        return tasks
            .Where(t => t.Status != TaskItemStatus.Done
            && t.DueDate is not null
            && t.DueDate.Value.Date >= today
            && t.DueDate.Value.Date <= limit)
            .OrderBy(t => t.DueDate)
            .ThenByDescending(t => t.Priority)
            .Select(t => FormatTask(t, now))
            .ToList();
    }

    protected override string FormatTask(TaskItem task, DateTime now)
    {
        int daysLeft = (task.DueDate!.Value.Date - now.Date).Days;
        return $"{base.FormatTask(task, now)}, осталось {daysLeft} дн.";
    }
}