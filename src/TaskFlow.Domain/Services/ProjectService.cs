using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Services;

public sealed class ProjectService(IProjectRepository repository, TimeProvider timeProvider) : IProjectService
{
    public Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default) =>
        repository.GetAllAsync(ct);

    public Task<Project?> GetByIdAsync(int id, CancellationToken ct = default) =>
        repository.GetByIdAsync(id, ct);

    public async Task<Project> CreateAsync(Project project, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(project);

        var existing = await repository.GetByNameAsync(project.Name, ct);
        if (existing is not null)
            throw new InvalidOperationException($"A project named '{project.Name}' already exists.");

        project.CreatedAt = timeProvider.GetUtcNow().UtcDateTime;
        await repository.AddAsync(project, ct);
        return project;
    }
}
