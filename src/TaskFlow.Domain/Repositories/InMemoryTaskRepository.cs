using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

public sealed class InMemoryTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks = new();
    private readonly Lock _sync = new();
    private int _lastId;

    public InMemoryTaskRepository()
    {
    }

    public InMemoryTaskRepository(IEnumerable<TaskItem> existingTasks)
    {
        _tasks.AddRange(existingTasks);
        _lastId = _tasks.Count == 0 ? 0 : _tasks.Max(t => t.Id);
    }

    public Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        lock (_sync)
        {
            IReadOnlyList<TaskItem> snapshot = _tasks.ToList();
            return Task.FromResult(snapshot);
        }
    }

    public Task<TaskItem?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        lock (_sync)
        {
            TaskItem? task = _tasks.FirstOrDefault(t => t.Id == id);
            return Task.FromResult(task);
        }
    }

    public Task AddAsync(TaskItem task, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ct.ThrowIfCancellationRequested();
        if (task.Id != 0)
        {
            throw new InvalidOperationException("Новая задача не должна иметь Id: его назначает репозиторий.");
        }

        lock (_sync)
        {
            _lastId++;
            task.Id = _lastId;
            _tasks.Add(task);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ct.ThrowIfCancellationRequested();
        lock (_sync)
        {
            int index = _tasks.FindIndex(t => t.Id == task.Id);
            if (index < 0)
            {
                throw new KeyNotFoundException($"Задача #{task.Id} не найдена.");
            }

            _tasks[index] = task;
        }

        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        lock (_sync)
        {
            bool removed = _tasks.RemoveAll(t => t.Id == id) > 0;
            return Task.FromResult(removed);
        }
    }
}
