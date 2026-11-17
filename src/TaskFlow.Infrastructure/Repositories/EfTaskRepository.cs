using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories;

public class EfTaskRepository(TaskFlowDbContext db) : ITaskRepository
{
    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Tasks
            .AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .OrderBy(t => t.Id)
            .ToListAsync(ct);
    }

    public async Task<TaskItem?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Tasks
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task AddAsync(TaskItem task, CancellationToken ct = default)
    {
        db.Tasks.Add(task);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        if (db.Entry(task).State == EntityState.Detached)
            db.Tasks.Update(task);

        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var task = await db.Tasks.FindAsync(new object[] { id }, ct);
        if (task is null)
            return false;

        db.Tasks.Remove(task);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<TaskItem>> SearchAsync(TaskFilter filter, CancellationToken ct = default)
    {
        IQueryable<TaskItem> query = db.Tasks
            .AsNoTracking()
            .Include(t => t.Tags);

        if (filter.Status is { } status)
            query = query.Where(t => t.Status == status);

        if (filter.Priority is { } priority)
            query = query.Where(t => t.Priority == priority);

        if (filter.ProjectId is { } projectId)
            query = query.Where(t => t.ProjectId == projectId);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(t =>
                t.Title.Contains(term) ||
                (t.Description != null && t.Description.Contains(term)));
        }

        return await query
            .OrderBy(t => t.Id)
            .ToListAsync(ct);
    }

    public async Task<TaskStats> GetStatsAsync(DateTime now, CancellationToken ct = default)
    {
        var tasks = db.Tasks.AsNoTracking();

        var total = await tasks.CountAsync(ct);

        var byStatusQuery = tasks
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() });

        

        var byStatus = await byStatusQuery.ToListAsync(ct);

        var byPriority = await tasks
            .GroupBy(t => t.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var overdue = await tasks.CountAsync(
            t => t.DueDate < now && t.Status != TaskItemStatus.Done, ct);

        return new TaskStats(
            total,
            byStatus.ToDictionary(x => x.Status, x => x.Count),
            byPriority.ToDictionary(x => x.Priority, x => x.Count),
            overdue);
    }
}