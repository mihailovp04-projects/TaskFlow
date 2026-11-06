using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Services;

public sealed class ProjectService(
    IProjectRepository repository,
    ITaskRepository taskRepository,
    TimeProvider timeProvider) : IProjectService

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
    public async Task<bool> UpdateAsync(int id, string name, string? description, CancellationToken ct = default)
    {
        var project = await repository.GetByIdAsync(id, ct);
        if (project is null)
            return false;

        var sameName = await repository.GetByNameAsync(name, ct);
        if (sameName is not null && sameName.Id != project.Id)
            throw new InvalidOperationException($"A project named '{name}' already exists.");

        project.Name = name;
        project.Description = description;
        await repository.UpdateAsync(project, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var project = await repository.GetByIdAsync(id, ct);
        if (project is null)
            return false;

        var tasks = await taskRepository.GetAllAsync(ct);
        if (tasks.Any(t => t.ProjectId == id))
            throw new InvalidOperationException($"Project {id} has tasks and cannot be deleted.");

        return await repository.DeleteAsync(id, ct);
    }
}
