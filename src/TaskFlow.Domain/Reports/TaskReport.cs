using System.Text;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Reports;

public abstract class TaskReport
{
    public abstract string Title { get; }

    public string Render(IEnumerable<TaskItem> tasks, DateTime now)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"=== {Title} ===");

        IReadOnlyList<string> lines = Generate(tasks, now);
        if (lines.Count == 0)
        {
            builder.AppendLine("(нет данных)");
        }
        else
        {
            foreach (string line in lines)
            {
                builder.AppendLine(line);
            }
        }

        builder.AppendLine($"Сформирован: {now:yyyy-MM-dd HH:mm}");
        return builder.ToString();
    }

    protected abstract IReadOnlyList<string> Generate(IEnumerable<TaskItem> tasks, DateTime now);

    protected virtual string FormatTask(TaskItem task, DateTime now) => task.ToString();
}
