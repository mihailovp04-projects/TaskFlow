using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

public sealed class InMemoryTagRepository : ITagRepository
{
    private readonly List<Tag> _tags = [];
    private readonly Lock _sync = new();
    private int _nextId = 1;

    public Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        lock (_sync)
        {
            IReadOnlyList<Tag> snapshot = _tags.OrderBy(p => p.Name).ToList();
            return Task.FromResult(snapshot);
        }
    }

    public Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        lock (_sync)
        {
            var tag = _tags.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(tag);
        }
    }

    public Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        ct.ThrowIfCancellationRequested();
        var trimmed = name.Trim();
        lock (_sync)
        {
            var tag = _tags.FirstOrDefault(
                p => string.Equals(p.Name, trimmed, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(tag);
        }
    }

    public Task AddAsync(Tag tag, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ct.ThrowIfCancellationRequested();
        lock (_sync)
        {
            tag.Id = _nextId++;
            _tags.Add(tag);
        }
        return Task.CompletedTask;
    }
}
