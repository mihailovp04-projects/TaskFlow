using TaskFlow.Shared.Dtos;
using TaskFlow.Web.Models;

namespace TaskFlow.Web.Services;

public static class TaskApiClientExtensions
{
    public static async Task ApplyStatusChangeAsync(
        this ITaskApiClient api,
        List<TaskDto>? tasks,
        StatusChangeRequest request,
        CancellationToken ct = default)
    {
        await api.ChangeStatusAsync(request.TaskId, request.NewStatus, ct);

        var updated = await api.GetTaskAsync(request.TaskId, ct);
        if (updated is null || tasks is null)
            return;

        var index = tasks.FindIndex(t => t.Id == updated.Id);
        if (index >= 0)
            tasks[index] = updated;
    }
}