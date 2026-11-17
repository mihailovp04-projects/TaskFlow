namespace TaskFlow.Domain.Abstractions;

public interface ITaskTagService
{
    Task<SetTaskTagsResult> SetTagsAsync(int taskId, IReadOnlyCollection<int> tagIds, CancellationToken ct = default);
}

public sealed record SetTaskTagsResult(bool TaskFound, IReadOnlyList<int> MissingTagIds);