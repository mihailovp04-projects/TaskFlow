using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Repositories;
using TaskFlow.Domain.Services;
using TaskFlow.Domain.Tests.Fakes;

namespace TaskFlow.Domain.Tests;

public class ProjectServiceTests
{
    private readonly InMemoryTaskRepository _taskRepository = new();
    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        _service = new ProjectService(
            new InMemoryProjectRepository(),
            _taskRepository,
            new FixedTimeProvider(new DateTime(2026, 10, 24, 9, 0, 0)));
    }

    [Fact]
    public async Task CreateAsync_AssignsId()
    {
        var created = await _service.CreateAsync(new Project("TaskFlow"));

        Assert.Equal(1, created.Id);
    }

    [Fact]
    public async Task CreateAsync_DuplicateNameIgnoringCase_Throws()
    {
        await _service.CreateAsync(new Project("TaskFlow"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(new Project("taskflow")));
    }

    [Fact]
    public async Task UpdateAsync_RenameToSameNameOtherCase_Succeeds()
    {
        var project = await _service.CreateAsync(new Project("TaskFlow"));

        var updated = await _service.UpdateAsync(project.Id, "taskflow", null);

        Assert.True(updated);
        var saved = await _service.GetByIdAsync(project.Id);
        Assert.NotNull(saved);
        Assert.Equal("taskflow", saved.Name);
    }

    [Fact]
    public async Task UpdateAsync_NameTakenByOtherProject_Throws()
    {
        await _service.CreateAsync(new Project("Alpha"));
        var beta = await _service.CreateAsync(new Project("Beta"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(beta.Id, "ALPHA", null));
    }

    [Fact]
    public async Task DeleteAsync_ProjectWithTasks_Throws()
    {
        var project = await _service.CreateAsync(new Project("Busy"));
        await _taskRepository.AddAsync(new TaskItem("Blocker") { ProjectId = project.Id });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteAsync(project.Id));
    }

    [Fact]
    public async Task DeleteAsync_EmptyProject_ReturnsTrue()
    {
        var project = await _service.CreateAsync(new Project("Empty"));

        var deleted = await _service.DeleteAsync(project.Id);

        Assert.True(deleted);
        Assert.Null(await _service.GetByIdAsync(project.Id));
    }
}