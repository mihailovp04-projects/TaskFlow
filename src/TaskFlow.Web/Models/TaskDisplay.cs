using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Dtos;

namespace TaskFlow.Web.Models;

// Общие правила отображения, чтобы не повторять их в каждом компоненте
public static class TaskDisplay
{
    public static string StatusLabel(TaskItemStatus status) => status switch
    {
        TaskItemStatus.Todo => "To do",
        TaskItemStatus.InProgress => "In progress",
        TaskItemStatus.Done => "Done",
        _ => status.ToString()
    };

    public static bool IsOverdue(TaskDto task, DateTime today) =>
        task.Status != TaskItemStatus.Done
        && task.DueDate is not null
        && task.DueDate.Value.Date < today;
}
