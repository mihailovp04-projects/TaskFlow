using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

public sealed class InMemoryTaskRepository : InMemoryRepository<TaskItem>, ITaskRepository
{
    public InMemoryTaskRepository()
    {
    }

    public InMemoryTaskRepository(IEnumerable<TaskItem> existingTasks)
        : base(existingTasks)
    {
    }
}