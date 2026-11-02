using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Repositories;
using TaskFlow.Domain.Services;
using TaskFlow.Domain.Tests.Fakes;

namespace TaskFlow.Domain.Tests;

public class TaskServiceTests
{
    private static readonly DateTime Today = new(2026, 10, 5, 9, 0, 0);

    private readonly InMemoryTaskRepository _repository = new();
    private readonly RecordingNotifier _notifier = new();
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _service = new TaskService(_repository, _notifier, new FixedTimeProvider(Today));
    }

    [Fact]
    public void Create_WithDueDateInPast_ThrowsArgumentException()
    {

        DateTime yesterday = Today.AddDays(-1);

        Assert.Throws<ArgumentException>(() => _service.Create("Старая задача", TaskPriority.Low, yesterday));
        Assert.Empty(_repository.GetAll());
    }

    [Fact]
    public void Complete_ExistingTask_SavesStatusAndSendsNotification()
    {

        var task = _service.Create("Подготовить демо", TaskPriority.Medium, Today.AddDays(2));
        _notifier.Messages.Clear();


        _service.Complete(task.Id);

        var saved = _repository.GetById(task.Id);
        Assert.NotNull(saved);
        Assert.Equal(TaskItemStatus.Done, saved.Status);
        Assert.Equal(Today, saved.CompletedAt);
        Assert.Single(_notifier.Messages);
        Assert.Contains($"#{task.Id}", _notifier.Messages[0]);
    }

    [Fact]
    public void Start_WhenMaxTasksInProgress_ThrowsInvalidOperationException()
    {
        // Arrange
        var first = _service.Create("Задача 1", TaskPriority.Low, null);
        var second = _service.Create("Задача 2", TaskPriority.Low, null);
        var third = _service.Create("Задача 3", TaskPriority.Low, null);
        var fourth = _service.Create("Задача 4", TaskPriority.Low, null);
        _service.Start(first.Id);
        _service.Start(second.Id);
        _service.Start(third.Id);

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => _service.Start(fourth.Id));
    }

    [Fact]
    public void Search_IgnoresCase()
    {
        // Arrange
        _service.Create("Подготовить Демо", TaskPriority.Medium, null);

        // Act
        var result = _service.Search("демо");

        // Assert
        var found = Assert.Single(result);
        Assert.Equal("Подготовить Демо", found.Title);
    }

    [Fact]
    public void Delete_NonExistingId_ReturnsFalseAndDoesNotNotify()
    {
        // Arrange
        int missingId = 99;

        // Act
        bool deleted = _service.Delete(missingId);

        // Assert
        Assert.False(deleted);
        Assert.Empty(_notifier.Messages);
    }

    [Fact]
    public void Filter_ByTodo_ReturnsOnlyTodoTasks()
    {
        // Arrange
        var started = _service.Create("В работе", TaskPriority.Low, null);
        var todo = _service.Create("Сделать", TaskPriority.Low, null);
        _service.Start(started.Id);

        // Act
        var result = _service.Filter(TaskItemStatus.Todo, null );

        // Assert
        var found = Assert.Single(result);
        Assert.Equal(todo.Id, found.Id);
    }
}
