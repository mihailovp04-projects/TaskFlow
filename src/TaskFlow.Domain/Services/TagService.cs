using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Services;

public sealed class TagService(
    ITagRepository repository,
    TimeProvider timeProvider) : ITagService

{
    public Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default) =>
    repository.GetAllAsync(ct);

    public Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default) =>
        repository.GetByIdAsync(id, ct);

    public async Task<Tag> CreateAsync(Tag tag, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(tag);

        var existing = await repository.GetByNameAsync(tag.Name, ct);
        if (existing is not null)
            throw new InvalidOperationException($"A project named '{tag.Name}' already exists.");

        tag.CreatedAt = timeProvider.GetUtcNow().UtcDateTime;
        await repository.AddAsync(tag, ct);
        return tag;
    }
}
