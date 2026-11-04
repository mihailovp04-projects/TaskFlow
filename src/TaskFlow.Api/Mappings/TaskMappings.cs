using TaskFlow.Domain.Entities;
using TaskFlow.Shared.Dtos;

namespace TaskFlow.Api.Mappings;

public static class TaskMappings
{
    public static TaskDto ToDto(this TaskItem task, DateTime now) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Priority,
        task.Status,
        task.DueDate,
        task.CompletedAt,
        task.ProjectId,
        task.CreatedAt,
        task.IsOverdue(now));

    public static TaskItem ToEntity(this CreateTaskRequest request) =>
        new(request.Title, request.Priority, request.DueDate, request.Description)
        {
            ProjectId = request.ProjectId
        };

    public static void ApplyTo(this UpdateTaskRequest request, TaskItem task)
    {
        task.Title = request.Title;
        task.Description = request.Description;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.ProjectId = request.ProjectId;
    }
}
