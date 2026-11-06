using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Abstractions;

public interface ITagRepository
{
    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default);
    Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(Tag tag, CancellationToken ct = default);
}
