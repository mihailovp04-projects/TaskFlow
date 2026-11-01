using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Abstractions;
public interface ITaskRepository
{
    IReadOnlyList<TaskItem> GetAll();

    TaskItem? GetById(int id);

    void Add(TaskItem task);

    void Update(TaskItem task);

    bool Delete(int id);
}
