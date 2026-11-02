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
    [Theory]
    [InlineData(-1, false, true)]
    [InlineData(0, false, false)]
    [InlineData(1, false, false)]
    [InlineData(-1, true, false)]

    public void IsOverdue_ReturnsExpected(int dueInDays, bool done, bool expected)
    {
        // Arrange
        var today = new DateTime(2026, 10, 5);
        var task = new TaskItem("Задача", TaskPriority.Low, today.AddDays(dueInDays));
        if (done)
        {
            task.Complete(today);
        }

        // Act
        bool actual = task.IsOverdue(today);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Reopen_WhenTodo_ThrowsInvalidOperationException()
    {
        // Arrange
        var task = new TaskItem("Купить мясо");
        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => task.Reopen());
    }
    public static TheoryData<string> InvalidTitles => new()
{
    "",
    "   ",
    new string('a', TaskItem.TitleMaxLength + 1)
};

    [Theory]
    [MemberData(nameof(InvalidTitles))]
    public void Constructor_WithInvalidTitle_ThrowsArgumentException(string title)
    {
        // Act + Assert
        Assert.Throws<ArgumentException>(() => new TaskItem(title));
    }


}
