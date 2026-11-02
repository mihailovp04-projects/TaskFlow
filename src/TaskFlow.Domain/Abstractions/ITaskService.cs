using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Abstractions;

public interface ITaskService
{
    TaskItem Create(string title, TaskPriority priority, DateTime? dueDate, string? description = null, int? projectId = null);

    IReadOnlyList<TaskItem> GetAll();

    IReadOnlyList<TaskItem> Search(string text);
    IReadOnlyList<TaskItem> Filter(TaskItemStatus? status, TaskPriority? priority);

    void Start(int id);

    void Complete(int id);

    void Reopen(int id);

    bool Delete(int id);

    bool HasOverdue();

    IReadOnlyDictionary<TaskItemStatus, int> CountByStatus();
}
