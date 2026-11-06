using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

public sealed class InMemoryProjectRepository : IProjectRepository
{
    private readonly List<Project> _projects = [];
    private readonly Lock _sync = new();
    private int _nextId = 1;

    public Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        lock (_sync)
        {
            IReadOnlyList<Project> snapshot = _projects.OrderBy(p => p.Id).ToList();
            return Task.FromResult(snapshot);
        }
    }

    public Task<Project?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        lock (_sync)
        {
            var project = _projects.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(project);
        }
    }

    public Task<Project?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        ct.ThrowIfCancellationRequested();
        var trimmed = name.Trim();
        lock (_sync)
        {
            var project = _projects.FirstOrDefault(
                p => string.Equals(p.Name, trimmed, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(project);
        }
    }

    public Task AddAsync(Project project, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        ct.ThrowIfCancellationRequested();
        lock (_sync)
        {
            project.Id = _nextId++;
            _projects.Add(project);
        }
        return Task.CompletedTask;
    }
    public Task UpdateAsync(Project project, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(project);
        ct.ThrowIfCancellationRequested();
        lock (_sync)
        {
            int index = _projects.FindIndex(p => p.Id == project.Id);
            if (index < 0)
            {
                throw new KeyNotFoundException($"Проект #{project.Id} не найден.");
            }

            _projects[index] = project;
        }

        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        lock (_sync)
        {
            bool removed = _projects.RemoveAll(t => t.Id == id) > 0;
            return Task.FromResult(removed);
        }
    }
}
