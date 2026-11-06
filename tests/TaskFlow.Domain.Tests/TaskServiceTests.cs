using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Repositories;
using TaskFlow.Domain.Services;
using TaskFlow.Domain.Tests.Fakes;

namespace TaskFlow.Domain.Tests;

public class TaskServiceTests
{
    private static readonly DateTime Now = new(2026, 10, 24, 9, 0, 0);

    private readonly InMemoryTaskRepository _repository = new();
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _service = new TaskService(_repository, new FixedTimeProvider(Now));
    }

    [Fact]
    public async Task CreateAsync_AssignsIdAndCreatedAt()
    {
        var created = await _service.CreateAsync(new TaskItem("Write tests"));

        Assert.Equal(1, created.Id);
        Assert.Equal(Now, created.CreatedAt);
        Assert.Equal(TaskItemStatus.Todo, created.Status);
    }

    [Fact]
    public async Task ChangeStatusAsync_TodoToInProgress_ChangesStatus()
    {
        var created = await _service.CreateAsync(new TaskItem("Start me"));

        var changed = await _service.ChangeStatusAsync(created.Id, TaskItemStatus.InProgress);

        Assert.NotNull(changed);
        Assert.Equal(TaskItemStatus.InProgress, changed.Status);
    }

    [Fact]
    public async Task ChangeStatusAsync_ToDone_SetsCompletedAt()
    {
        var created = await _service.CreateAsync(new TaskItem("Finish me"));

        var changed = await _service.ChangeStatusAsync(created.Id, TaskItemStatus.Done);

        Assert.NotNull(changed);
        Assert.Equal(TaskItemStatus.Done, changed.Status);
        Assert.Equal(Now, changed.CompletedAt);
    }

    [Fact]
    public async Task ChangeStatusAsync_DoneToInProgress_Throws()
    {
        var created = await _service.CreateAsync(new TaskItem("Already done"));
        await _service.ChangeStatusAsync(created.Id, TaskItemStatus.Done);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ChangeStatusAsync(created.Id, TaskItemStatus.InProgress));
    }

    [Fact]
    public async Task ChangeStatusAsync_UnknownId_ReturnsNull()
    {
        var changed = await _service.ChangeStatusAsync(42, TaskItemStatus.Done);

        Assert.Null(changed);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByStatusAndSearch()
    {
        var api = await _service.CreateAsync(new TaskItem("Write API"));
        await _service.CreateAsync(new TaskItem("Write docs"));
        await _service.CreateAsync(new TaskItem("Buy milk"));
        await _service.ChangeStatusAsync(api.Id, TaskItemStatus.InProgress);

        var result = await _service.GetAllAsync(new TaskFilter(Status: TaskItemStatus.Todo, Search: "write"));

        var single = Assert.Single(result);
        Assert.Equal("Write docs", single.Title);
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ReturnsFalse()
    {
        var deleted = await _service.DeleteAsync(42);

        Assert.False(deleted);
    }
    [Fact]
    public async Task ChangeStatusAsync_InProgressToTodo_Throws()
    {
        var created = await _service.CreateAsync(new TaskItem("In progress"));
        await _service.ChangeStatusAsync(created.Id, TaskItemStatus.InProgress);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ChangeStatusAsync(created.Id, TaskItemStatus.Todo));
    }

    [Fact]
    public async Task ChangeStatusAsync_DoneToTodo_ResetsCompletedAt()
    {
        var created = await _service.CreateAsync(new TaskItem("Reopen me"));
        await _service.ChangeStatusAsync(created.Id, TaskItemStatus.Done);

        var changed = await _service.ChangeStatusAsync(created.Id, TaskItemStatus.Todo);

        Assert.NotNull(changed);
        Assert.Equal(TaskItemStatus.Todo, changed.Status);
        Assert.Null(changed.CompletedAt);
    }

    [Fact]
    public async Task ChangeStatusAsync_InProgressToDone_IsAllowed()
    {
        var created = await _service.CreateAsync(new TaskItem("Finish from progress"));
        await _service.ChangeStatusAsync(created.Id, TaskItemStatus.InProgress);

        var changed = await _service.ChangeStatusAsync(created.Id, TaskItemStatus.Done);

        Assert.NotNull(changed);
        Assert.Equal(TaskItemStatus.Done, changed.Status);
        Assert.Equal(Now, changed.CompletedAt);
    }
}
