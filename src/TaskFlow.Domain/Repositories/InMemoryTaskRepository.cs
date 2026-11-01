using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

public sealed class InMemoryTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks = new();
    private int _lastId;

    public InMemoryTaskRepository()
    {
    }

    public InMemoryTaskRepository(IEnumerable<TaskItem> existingTasks)
    {
        _tasks.AddRange(existingTasks);
        _lastId = _tasks.Count == 0 ? 0 : _tasks.Max(t => t.Id);
    }

    public IReadOnlyList<TaskItem> GetAll() => _tasks.ToList();

    public TaskItem? GetById(int id) => _tasks.FirstOrDefault(t => t.Id == id);

    public void Add(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);
        if (task.Id != 0)
        {
            throw new InvalidOperationException("Новая задача не должна иметь Id: его назначает репозиторий.");
        }

        _lastId++;
        task.Id = _lastId;
        _tasks.Add(task);
    }

    public void Update(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);
        int index = _tasks.FindIndex(t => t.Id == task.Id);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Задача #{task.Id} не найдена.");
        }

        _tasks[index] = task;
    }

    public bool Delete(int id) => _tasks.RemoveAll(t => t.Id == id) > 0;
}
