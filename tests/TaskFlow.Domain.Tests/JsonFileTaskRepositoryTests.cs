using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Repositories;

namespace TaskFlow.Domain.Tests;

public class JsonFileTaskRepositoryTests
{
    [Fact]
    public async Task Add_ThenNewInstance_ReadsSameTasksFromFile()
    {
        // Arrange
        string filePath = Path.Combine(Path.GetTempPath(), $"taskflow-{Guid.NewGuid()}.json");
        try
        {
            var first = new JsonFileTaskRepository(filePath);
            var task = new TaskItem("Проверить JSON", TaskPriority.Critical, new DateTime(2026, 12, 31));
            task.Start();

            // Act
            await first.AddAsync(task);
            var second = new JsonFileTaskRepository(filePath);

            // Assert
            var loaded = Assert.Single(await second.GetAllAsync());
            Assert.Equal(1, loaded.Id);
            Assert.Equal("Проверить JSON", loaded.Title);
            Assert.Equal(TaskPriority.Critical, loaded.Priority);
            Assert.Equal(TaskItemStatus.InProgress, loaded.Status);
            Assert.Equal(new DateTime(2026, 12, 31), loaded.DueDate);
        }
        finally
        {
            File.Delete(filePath);
        }
    }
}
