using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories;

public class EfTagRepository(TaskFlowDbContext db) : ITagRepository
{
    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        var trimmed = name.Trim();

        return db.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Name == trimmed, ct);
    }

    public async Task<IReadOnlyList<TagSummary>> GetSummariesAsync(CancellationToken ct = default)
    {
        return await db.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TagSummary(t.Id, t.Name, t.Tasks.Count))
            .ToListAsync(ct);
    }
    public Task<TagSummary?> GetSummaryByIdAsync(int id, CancellationToken ct = default) =>
    db.Tags
        .AsNoTracking()
        .Where(t => t.Id == id)
        .Select(t => new TagSummary(t.Id, t.Name, t.Tasks.Count))
        .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Tag>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        return await db.Tags
            .Where(t => ids.Contains(t.Id))
            .ToListAsync(ct);
    }

    public async Task AddAsync(Tag tag, CancellationToken ct = default)
    {
        db.Tags.Add(tag);
        await db.SaveChangesAsync(ct);
    }
}