using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Repositories;
using TaskFlow.Domain.Services;
using TaskFlow.Domain.Tests.Fakes;

namespace TaskFlow.Domain.Tests;

public class TagServiceTests
{
    private static readonly DateTime Now = new(2026, 10, 24, 9, 0, 0);

    private readonly TagService _service = new(
        new InMemoryTagRepository(),
        new FixedTimeProvider(Now));

    [Fact]
    public async Task CreateAsync_AssignsIdAndCreatedAt()
    {
        var created = await _service.CreateAsync(new Tag("urgent"));

        Assert.Equal(1, created.Id);
        Assert.Equal(Now, created.CreatedAt);
    }

    [Fact]
    public async Task CreateAsync_DuplicateNameIgnoringCase_Throws()
    {
        await _service.CreateAsync(new Tag("urgent"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(new Tag("URGENT")));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsTagsSortedByName()
    {
        await _service.CreateAsync(new Tag("urgent"));
        await _service.CreateAsync(new Tag("backend"));
        await _service.CreateAsync(new Tag("frontend"));

        var tags = await _service.GetAllAsync();

        Assert.Equal(new[] { "backend", "frontend", "urgent" }, tags.Select(t => t.Name));
    }
}