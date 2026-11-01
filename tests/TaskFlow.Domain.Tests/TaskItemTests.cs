using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Tests;

public class TaskItemTests
{
    [Fact]
    public void Complete_FromTodo_SetsDoneAndCompletedAt()
    {
        // Arrange
        var task = new TaskItem("Написать отчёт", TaskPriority.High);
        var now = new DateTime(2026, 10, 5, 14, 30, 0);

        // Act
        task.Complete(now);

        // Assert
        Assert.Equal(TaskItemStatus.Done, task.Status);
        Assert.Equal(now, task.CompletedAt);
    }

    [Fact]
    public void Start_WhenAlreadyDone_ThrowsInvalidOperationException()
    {
        // Arrange
        var task = new TaskItem("Купить кофе");
        task.Complete(new DateTime(2026, 10, 5));

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => task.Start());
    }
}
