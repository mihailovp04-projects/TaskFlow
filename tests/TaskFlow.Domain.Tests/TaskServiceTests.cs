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
}
