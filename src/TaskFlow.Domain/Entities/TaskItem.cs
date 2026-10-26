using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class TaskItem
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    private string _title = string.Empty;
    private string? _description;
    private TaskPriority _priority;

    public TaskItem(string title, TaskPriority priority = TaskPriority.Medium, DateTime? dueDate = null, string? description = null)
    {
        Title = title;
        Priority = priority;
        DueDate = dueDate;
        Description = description;
        Status = TaskItemStatus.Todo;
        CreatedAt = DateTime.Now;
    }

    public int Id { get; set; }

    public DateTime CreatedAt { get; private set; }

    public string Title
    {
        get => _title;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Title is required.", nameof(Title));
            }

            var trimmed = value.Trim();
            if (trimmed.Length > TitleMaxLength)
            {
                throw new ArgumentException($"Title must be at most {TitleMaxLength} characters.", nameof(Title));
            }

            _title = trimmed;
        }
    }

    public string? Description
    {
        get => _description;
        set
        {
            if (value is not null && value.Length > DescriptionMaxLength)
            {
                throw new ArgumentException($"Description must be at most {DescriptionMaxLength} characters.", nameof(Description));
            }
            _description = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }

    public TaskPriority Priority
    {
        get => _priority;
        set
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(nameof(Priority), value, "Unknown priority.");
            }

            _priority = value;
        }
    }
    public TaskItemStatus Status { get; private set; }

    public DateTime? DueDate { get; set; }

    public DateTime? CompletedAt { get; private set; }

    public int? ProjectId { get; set; }

    public Project? Project { get; set; }

    public void Start()
    {
        if (Status != TaskItemStatus.Todo)
        {
            throw new InvalidOperationException($"Only a task in status Todo can be started. Current status: {Status}.");
        }

        Status = TaskItemStatus.InProgress;
    }
    public void Complete(DateTime now)
    {
        if (Status == TaskItemStatus.Done)
        {
            throw new InvalidOperationException("The task is already done.");
        }

        Status = TaskItemStatus.Done;
        CompletedAt = now;
    }

    public void Reopen()
    {
        if (Status != TaskItemStatus.Done)
        {
            throw new InvalidOperationException($"Only a done task can be reopened. Current status: {Status}.");
        }

        Status = TaskItemStatus.Todo;
        CompletedAt = null;
    }
    public bool IsOverdue(DateTime now)
    {
        return Status != TaskItemStatus.Done && DueDate.HasValue && DueDate.Value.Date < now.Date;
    }

    public override string ToString()
    {
        var due = DueDate.HasValue ? DueDate.Value.ToString("yyyy-MM-dd") : "-";
        return $"#{Id} [{Status}] {Title} (priority: {Priority}, due: {due})";
    }
}
