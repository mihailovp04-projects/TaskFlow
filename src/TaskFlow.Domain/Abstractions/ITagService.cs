using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Abstractions;

public interface ITagService
{
    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default);
    Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Tag> CreateAsync(Tag tag, CancellationToken ct = default);
    Task<IReadOnlyList<TagSummary>> GetSummariesAsync(CancellationToken ct = default);
    Task<TagSummary?> GetSummaryByIdAsync(int id, CancellationToken ct = default);
}
