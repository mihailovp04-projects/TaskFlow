using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Repositories;

namespace TaskFlow.Domain.Tests;

public class InMemoryTaskRepositoryTests
{
    [Fact]
    public void Add_ThreeTasks_AssignsSequentialIds()
    {
        // Arrange
        var repository = new InMemoryTaskRepository();
        var first = new TaskItem("Первая");
        var second = new TaskItem("Вторая");
        var third = new TaskItem("Третья");

        // Act
        repository.Add(first);
        repository.Add(second);
        repository.Add(third);

        // Assert
        Assert.Equal(1, first.Id);
        Assert.Equal(2,second.Id);
        Assert.Equal(3, third.Id);
    }
}