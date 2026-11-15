using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(TaskFlowDbContext db, CancellationToken ct = default)
    {
        // Идемпотентность: если задачи уже есть, ничего не делаем
        if (await db.Tasks.AnyAsync(ct))
            return;

        // Теги из HasData, загружаем с отслеживанием, чтобы связать с задачами
        var tags = await db.Tags.ToDictionaryAsync(t => t.Name, ct);
        var now = DateTime.UtcNow;

        var setupEf = new TaskItem("Set up EF Core", TaskPriority.High, now.AddDays(2), "Infrastructure project and migrations") { ProjectId = 2, CreatedAt = now };
        setupEf.Tags.Add(tags["backend"]);
        setupEf.Start();

        var designUi = new TaskItem("Sketch the task list page", TaskPriority.Medium, now.AddDays(10)) { ProjectId = 2, CreatedAt = now };
        designUi.Tags.Add(tags["frontend"]);

        var fixBug = new TaskItem("Fix overdue calculation", TaskPriority.Critical, now.AddDays(-1), "Reported by QA") { ProjectId = 2, CreatedAt = now };
        fixBug.Tags.Add(tags["backend"]);
        fixBug.Tags.Add(tags["urgent"]);

        var readDocs = new TaskItem("Read EF Core docs", TaskPriority.Low) { ProjectId = 1, CreatedAt = now };
        readDocs.Complete(now);

        var buyCoffee = new TaskItem("Buy coffee") { CreatedAt = now };

        db.Tasks.AddRange(setupEf, designUi, fixBug, readDocs, buyCoffee);

        // Один SaveChanges: 5 INSERT в Tasks и 4 INSERT в TaskItemTags в одной транзакции
        await db.SaveChangesAsync(ct);
    }
}
