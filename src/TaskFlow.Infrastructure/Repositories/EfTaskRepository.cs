using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Infrastructure.Repositories;

public class EfTaskRepository(TaskFlowDbContext db) : ITaskRepository
{
    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken ct = default)
    {
        // Только чтение: без отслеживания, связанные данные одним запросом с JOIN
        return await db.Tasks
            .AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .OrderBy(t => t.Id)
            .ToListAsync(ct);
    }

    public async Task<TaskItem?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        // С отслеживанием: сервис может изменить объект и вызвать UpdateAsync
        return await db.Tasks
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task AddAsync(TaskItem task, CancellationToken ct = default)
    {
        db.Tasks.Add(task);
        // После INSERT EF записывает сгенерированный Id в task.Id
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        // Объект из GetByIdAsync уже отслеживается; объект «со стороны» нужно присоединить
        if (db.Entry(task).State == EntityState.Detached)
            db.Tasks.Update(task);

        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var task = await db.Tasks.FindAsync(new object[] { id }, ct);
        if (task is null)
            return false;

        // Строки TaskItemTags удалит каскад ON DELETE CASCADE в базе
        db.Tasks.Remove(task);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
