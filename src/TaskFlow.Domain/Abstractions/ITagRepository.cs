using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Abstractions;

public interface ITagRepository
{
    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default);
    Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(Tag tag, CancellationToken ct = default);
    Task<IReadOnlyList<TagSummary>> GetSummariesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Tag>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
    Task<TagSummary?> GetSummaryByIdAsync(int id, CancellationToken ct = default);
}
