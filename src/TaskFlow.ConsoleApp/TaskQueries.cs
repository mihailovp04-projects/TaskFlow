using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.ConsoleApp;

public static class TaskQueries
{
    public static List<TaskItem> FindOverdue(List<TaskItem> tasks, DateTime now)
    {
        var result = new List<TaskItem>();
        foreach (var task in tasks)
        {
            if (task.IsOverdue(now))
            {
                result.Add(task);
            }
        }

        return result;
    }

    public static List<TaskItem> FindByPriority(List<TaskItem> tasks, TaskPriority priority)
    {
        var result = new List<TaskItem>();
        foreach (var task in tasks)
        {
            if (task.Priority == priority)
            {
                result.Add(task);
            }
        }

        return result;
    }

    public static List<TaskItem> FindByStatus(List<TaskItem> tasks, TaskItemStatus status)
    {
        var result = new List<TaskItem>();
        foreach (var task in tasks)
        {
            if (task.Status == status)
            {
                result.Add(task);
            }
        }

        return result;
    }

    public static List<TaskItem> FindByText(List<TaskItem> tasks, string text)
    {
        var result = new List<TaskItem>();
        foreach (var task in tasks)
        {
            var inTitle = task.Title.Contains(text, StringComparison.OrdinalIgnoreCase);
            var inDescription = task.Description is not null
                && task.Description.Contains(text, StringComparison.OrdinalIgnoreCase);

            if (inTitle || inDescription)
            {
                result.Add(task);
            }
        }

        return result;
    }

    public static Dictionary<TaskItemStatus, int> CountByStatus(List<TaskItem> tasks)
    {
        var counts = new Dictionary<TaskItemStatus, int>();
        foreach (var status in Enum.GetValues<TaskItemStatus>())
        {
            counts[status] = 0;
        }

        foreach (var task in tasks)
        {
            counts[task.Status]++;
        }

        return counts;
    }

    public static List<TaskItem> SortByDueDateThenPriority(List<TaskItem> tasks)
    {
        var sorted = new List<TaskItem>(tasks);
        sorted.Sort(CompareByDueDateThenPriority);
        return sorted;
    }

    private static int CompareByDueDateThenPriority(TaskItem a, TaskItem b)
    {
        if (a.DueDate.HasValue && !b.DueDate.HasValue)
        {
            return -1;
        }

        if (!a.DueDate.HasValue && b.DueDate.HasValue)
        {
            return 1;
        }

        if (a.DueDate.HasValue && b.DueDate.HasValue)
        {
            var byDate = a.DueDate.Value.Date.CompareTo(b.DueDate.Value.Date);
            if (byDate != 0)
            {
                return byDate;
            }
        }

        return b.Priority.CompareTo(a.Priority);
    }
}