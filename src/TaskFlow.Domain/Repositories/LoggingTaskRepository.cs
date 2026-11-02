using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

public sealed class LoggingTaskRepository : ITaskRepository
{
    private readonly ITaskRepository _inner;
    private readonly INotifier _notifier;

    public LoggingTaskRepository(ITaskRepository inner, INotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(notifier);

        _inner = inner;
        _notifier = notifier;
    }

    public IReadOnlyList<TaskItem> GetAll() => _inner.GetAll();

    public TaskItem? GetById(int id) => _inner.GetById(id);

    public void Add(TaskItem task)
    {
        _inner.Add(task);
        _notifier.Notify($"Репозиторий: добавлена запись #{task.Id}");
    }

    public void Update(TaskItem task)
    {
        _inner.Update(task);
        _notifier.Notify($"Репозиторий: обновлена запись #{task.Id}");
    }

    public bool Delete(int id)
    {
        bool deleted = _inner.Delete(id);
        if (deleted)
        {
            _notifier.Notify($"Репозиторий: удалена запись #{id}");
        }

        return deleted;
    }
}