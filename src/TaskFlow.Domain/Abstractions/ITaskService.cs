using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Abstractions;

public interface ITaskService
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync(TaskFilter filter, CancellationToken ct = default);
    Task<TaskItem?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default);
    Task<bool> UpdateAsync(TaskItem task, CancellationToken ct = default);
    Task<TaskItem?> ChangeStatusAsync(int id, TaskItemStatus newStatus, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
