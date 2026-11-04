using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Repositories;
using TaskFlow.Domain.Services;
using TaskFlow.Domain.Tests.Fakes;

namespace TaskFlow.Domain.Tests;

public class ProjectServiceTests
{
    private readonly ProjectService _service = new(
        new InMemoryProjectRepository(),
        new FixedTimeProvider(new DateTime(2026, 10, 24, 9, 0, 0)));

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
}
