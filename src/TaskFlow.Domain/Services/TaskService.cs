using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Services;

public sealed class TaskService : ITaskService
{
    public const int MaxTasksInProgress = 3;

    private readonly ITaskRepository _repository;
    private readonly INotifier _notifier;
    private readonly TimeProvider _clock;

    public TaskService(ITaskRepository repository, INotifier notifier, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(notifier);
        ArgumentNullException.ThrowIfNull(clock);

        _repository = repository;
        _notifier = notifier;
        _clock = clock;
    }

    private DateTime Now => _clock.GetLocalNow().DateTime;

    public TaskItem Create(string title, TaskPriority priority, DateTime? dueDate, string? description = null, int? projectId = null)
    {
        if (dueDate is not null && dueDate.Value.Date < Now.Date)
        {
            throw new ArgumentException("Срок не может быть в прошлом.", nameof(dueDate));
        }

        var task = new TaskItem(title, priority, dueDate, description)
        {
            CreatedAt = Now,
            ProjectId = projectId
        };

        _repository.Add(task);
        _notifier.Notify($"Создана задача #{task.Id}: {task.Title}");
        return task;
    }

    public IReadOnlyList<TaskItem> GetAll() =>
        _repository.GetAll()
            .OrderBy(t => t.Status)
            .ThenByDescending(t => t.Priority)
            .ThenBy(t => t.Id)
            .ToList();

    public IReadOnlyList<TaskItem> Search(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return GetAll();
        }

        string term = text.Trim();
        return _repository.GetAll()
            .Where(t => t.Title.Contains(term, StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => t.Id)
            .ToList();
    }

    public IReadOnlyList<TaskItem> Filter(TaskItemStatus? status, TaskPriority? priority) =>
    _repository.GetAll()
        .Where(t => (status is null || t.Status == status)
                 && (priority is null || t.Priority == priority))
        .OrderBy(t => t.Id)
        .ToList();

    public void Start(int id)
    {
        TaskItem task = GetRequired(id);

        int inProgress = _repository.GetAll().Count(t => t.Status == TaskItemStatus.InProgress);
        if (inProgress >= MaxTasksInProgress)
        {
            throw new InvalidOperationException($"Нельзя держать в работе больше {MaxTasksInProgress} задач одновременно.");
        }

        task.Start();
        _repository.Update(task);
        _notifier.Notify($"Задача #{task.Id} взята в работу");
    }

    public void Complete(int id)
    {
        TaskItem task = GetRequired(id);
        task.Complete(Now);
        _repository.Update(task);
        _notifier.Notify($"Задача #{task.Id} выполнена");
    }

    public void Reopen(int id)
    {
        TaskItem task = GetRequired(id);
        task.Reopen();
        _repository.Update(task);
        _notifier.Notify($"Задача #{task.Id} возвращена в работу");
    }

    public bool Delete(int id)
    {
        bool deleted = _repository.Delete(id);
        if (deleted)
        {
            _notifier.Notify($"Задача #{id} удалена");
        }

        return deleted;
    }

    public bool HasOverdue() => _repository.GetAll().Any(t => t.IsOverdue(Now));

    public IReadOnlyDictionary<TaskItemStatus, int> CountByStatus() =>
        _repository.GetAll()
            .GroupBy(t => t.Status)
            .ToDictionary(g => g.Key, g => g.Count());

    private TaskItem GetRequired(int id) =>
        _repository.GetById(id) ?? throw new KeyNotFoundException($"Задача #{id} не найдена.");
}
