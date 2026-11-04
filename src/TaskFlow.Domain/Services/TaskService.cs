using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Services;

public sealed class TaskService(ITaskRepository repository, TimeProvider timeProvider) : ITaskService
{
    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(TaskFilter filter, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var tasks = await repository.GetAllAsync(ct);
        IEnumerable<TaskItem> query = tasks;

        if (filter.Status is { } status)
            query = query.Where(t => t.Status == status);
        if (filter.Priority is { } priority)
            query = query.Where(t => t.Priority == priority);
        if (filter.ProjectId is { } projectId)
            query = query.Where(t => t.ProjectId == projectId);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(t =>
                t.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (t.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return query.OrderBy(t => t.Id).ToList();
    }

    public Task<TaskItem?> GetByIdAsync(int id, CancellationToken ct = default) =>
        repository.GetByIdAsync(id, ct);

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        task.CreatedAt = timeProvider.GetUtcNow().UtcDateTime;
        await repository.AddAsync(task, ct);
        return task;
    }

    public async Task<bool> UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        var existing = await repository.GetByIdAsync(task.Id, ct);
        if (existing is null)
            return false;

        await repository.UpdateAsync(task, ct);
        return true;
    }

    public async Task<TaskItem?> ChangeStatusAsync(int id, TaskItemStatus newStatus, CancellationToken ct = default)
    {
        var task = await repository.GetByIdAsync(id, ct);
        if (task is null)
            return null;

        switch (newStatus)
        {
            case TaskItemStatus.InProgress:
                task.Start();
                break;
            case TaskItemStatus.Done:
                task.Complete(timeProvider.GetUtcNow().UtcDateTime);
                break;
            case TaskItemStatus.Todo:
                task.Reopen();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newStatus), newStatus, "Unknown task status.");
        }

        await repository.UpdateAsync(task, ct);
        return task;
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default) =>
        repository.DeleteAsync(id, ct);
}
