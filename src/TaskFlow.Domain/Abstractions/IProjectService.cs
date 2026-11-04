using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Abstractions;

public interface IProjectService
{
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default);
    Task<Project?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Project> CreateAsync(Project project, CancellationToken ct = default);
}
