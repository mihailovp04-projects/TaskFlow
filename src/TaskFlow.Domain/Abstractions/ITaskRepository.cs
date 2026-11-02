using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Abstractions;

public interface ITaskRepository : IRepository<TaskItem>
{
}