using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Abstractions;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default);
    Task<Project?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Project?> GetByNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(Project project, CancellationToken ct = default);
}
