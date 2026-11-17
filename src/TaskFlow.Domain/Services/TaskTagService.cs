using TaskFlow.Domain.Abstractions;

namespace TaskFlow.Domain.Services;

public sealed class TaskTagService(
    ITaskRepository taskRepository,
    ITagRepository tagRepository) : ITaskTagService
{
    public async Task<SetTaskTagsResult> SetTagsAsync(int taskId, IReadOnlyCollection<int> tagIds, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(tagIds);

        var task = await taskRepository.GetByIdAsync(taskId, ct);
        if (task is null)
            return new SetTaskTagsResult(false, []);

        var ids = tagIds.Distinct().ToList();
        var tags = await tagRepository.GetByIdsAsync(ids, ct);

        var missing = ids.Except(tags.Select(t => t.Id)).Order().ToList();
        if (missing.Count > 0)
            return new SetTaskTagsResult(true, missing);

        var toRemove = task.Tags.Where(t => !ids.Contains(t.Id)).ToList();
        foreach (var tag in toRemove)
            task.Tags.Remove(tag);

        var currentIds = task.Tags.Select(t => t.Id).ToHashSet();
        foreach (var tag in tags.Where(t => !currentIds.Contains(t.Id)))
            task.Tags.Add(tag);

        await taskRepository.UpdateAsync(task, ct);
        return new SetTaskTagsResult(true, []);
    }
}