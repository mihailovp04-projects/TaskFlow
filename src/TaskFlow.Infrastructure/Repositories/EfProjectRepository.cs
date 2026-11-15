using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories;

public class EfProjectRepository(TaskFlowDbContext db) : IProjectRepository
{
    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Projects
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync(ct);
    }

    public Task<Project?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Project?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        var trimmed = name.Trim();

        // Регистр не учитывает SQL Server: стандартная сортировка (collation) базы не различает регистр
        return db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Name == trimmed, ct);
    }

    public async Task AddAsync(Project project, CancellationToken ct = default)
    {
        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Project project, CancellationToken ct = default)
    {
        if (db.Entry(project).State == EntityState.Detached)
            db.Projects.Update(project);

        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var project = await db.Projects.FindAsync(new object[] { id }, ct);
        if (project is null)
            return false;

        db.Projects.Remove(project);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
