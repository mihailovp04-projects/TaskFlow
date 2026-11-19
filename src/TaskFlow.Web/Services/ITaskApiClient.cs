using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Dtos;

namespace TaskFlow.Web.Services;

// Всё, что клиенту нужно от TaskFlow.Api. Неделя 6: FakeTaskApiClient, неделя 8: HttpTaskApiClient.
public interface ITaskApiClient
{
    Task<IReadOnlyList<TaskDto>> GetTasksAsync(
        TaskItemStatus? status = null,
        TaskPriority? priority = null,
        int? projectId = null,
        string? search = null,
        CancellationToken ct = default);

    // null, если задачи нет (у API это 404)
    Task<TaskDto?> GetTaskAsync(int id, CancellationToken ct = default);

    Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, CancellationToken ct = default);

    // false, если задачи нет (у API это 404)
    Task<bool> UpdateTaskAsync(int id, UpdateTaskRequest request, CancellationToken ct = default);

    Task<bool> ChangeStatusAsync(int id, TaskItemStatus status, CancellationToken ct = default);

    Task<bool> DeleteTaskAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(CancellationToken ct = default);

    // InvalidOperationException, если проект с таким именем уже есть (у API это 409)
    Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, CancellationToken ct = default);
}
