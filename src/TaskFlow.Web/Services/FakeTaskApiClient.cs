using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Dtos;

namespace TaskFlow.Web.Services;

// Реализация в памяти: имитирует API с задержкой сети, чтобы были видны состояния загрузки
public sealed class FakeTaskApiClient : ITaskApiClient
{
    private static readonly TimeSpan Latency = TimeSpan.FromMilliseconds(400);

    private readonly List<TaskDto> _tasks = [];
    private readonly List<ProjectDto> _projects = [];
    private int _nextTaskId = 1;
    private int _nextProjectId = 1;

    public FakeTaskApiClient()
    {
        Seed();
    }

    public async Task<IReadOnlyList<TaskDto>> GetTasksAsync(
        TaskItemStatus? status = null,
        TaskPriority? priority = null,
        int? projectId = null,
        string? search = null,
        CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);

        IEnumerable<TaskDto> query = _tasks;

        if (status is not null)
        {
            query = query.Where(t => t.Status == status);
        }

        if (priority is not null)
        {
            query = query.Where(t => t.Priority == priority);
        }

        if (projectId is not null)
        {
            query = query.Where(t => t.ProjectId == projectId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(t => t.Title.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        // сначала незавершённые, внутри статуса - более важные
        return query
            .OrderBy(t => t.Status)
            .ThenByDescending(t => t.Priority)
            .ThenBy(t => t.Id)
            .ToList();
    }

    public async Task<TaskDto?> GetTaskAsync(int id, CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);
        return _tasks.FirstOrDefault(t => t.Id == id);
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);

        var task = new TaskDto
        {
            Id = _nextTaskId++,
            Title = request.Title.Trim(),
            Description = request.Description,
            Priority = request.Priority,
            Status = TaskItemStatus.Todo,
            DueDate = request.DueDate,
            ProjectId = request.ProjectId,
            CreatedAt = DateTime.UtcNow
        };

        _tasks.Add(task);
        return task;
    }

    public async Task<bool> UpdateTaskAsync(int id, UpdateTaskRequest request, CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);

        var index = _tasks.FindIndex(t => t.Id == id);
        if (index < 0)
        {
            return false;
        }

        // record неизменяем: создаём копию с новыми значениями
        _tasks[index] = _tasks[index] with
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            ProjectId = request.ProjectId
        };
        return true;
    }

    public async Task<bool> ChangeStatusAsync(int id, TaskItemStatus status, CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);

        var index = _tasks.FindIndex(t => t.Id == id);
        if (index < 0)
        {
            return false;
        }

        _tasks[index] = _tasks[index] with
        {
            Status = status,
            CompletedAt = status == TaskItemStatus.Done ? DateTime.UtcNow : null
        };
        return true;
    }

    public async Task<bool> DeleteTaskAsync(int id, CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);
        return _tasks.RemoveAll(t => t.Id == id) > 0;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);
        return _projects.OrderBy(p => p.Name).ToList();
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, CancellationToken ct = default)
    {
        await Task.Delay(Latency, ct);

        var name = request.Name.Trim();
        if (_projects.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Project '{name}' already exists.");
        }

        return AddProject(name, request.Description);
    }

    private void Seed()
    {
        var website = AddProject("Website redesign", "New landing page and blog");
        var mobile = AddProject("Mobile app", "MVP for Android and iOS");
        AddProject("Internal tools", null);

        var today = DateTime.Today;
        AddTask("Prepare wireframes", "Home, pricing and contact pages", TaskPriority.High, TaskItemStatus.Done, today.AddDays(-5), website.Id);
        AddTask("Choose color palette", null, TaskPriority.Low, TaskItemStatus.Done, today.AddDays(-3), website.Id);
        AddTask("Implement landing page", "Responsive layout with Bootstrap 5", TaskPriority.Critical, TaskItemStatus.InProgress, today.AddDays(2), website.Id);
        AddTask("Write blog post about launch", null, TaskPriority.Medium, TaskItemStatus.Todo, today.AddDays(10), website.Id);
        AddTask("Set up push notifications", "Firebase for Android, APNs for iOS", TaskPriority.High, TaskItemStatus.Todo, today.AddDays(-1), mobile.Id);
        AddTask("Login screen", null, TaskPriority.Critical, TaskItemStatus.InProgress, today.AddDays(1), mobile.Id);
        AddTask("Renew domain", "Expires at the end of the month", TaskPriority.Medium, TaskItemStatus.Todo, today.AddDays(-2), null);
        AddTask("Read about Blazor lifecycle", null, TaskPriority.Low, TaskItemStatus.Todo, null, null);
    }

    private ProjectDto AddProject(string name, string? description)
    {
        var project = new ProjectDto { Id = _nextProjectId++, Name = name, Description = description };
        _projects.Add(project);
        return project;
    }

    private void AddTask(string title, string? description, TaskPriority priority, TaskItemStatus status, DateTime? dueDate, int? projectId)
    {
        _tasks.Add(new TaskDto
        {
            Id = _nextTaskId++,
            Title = title,
            Description = description,
            Priority = priority,
            Status = status,
            DueDate = dueDate,
            CompletedAt = status == TaskItemStatus.Done ? DateTime.UtcNow.AddDays(-1) : null,
            ProjectId = projectId,
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        });
    }
}
