using System.Text.Json.Serialization;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class TaskItem : BaseEntity
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    private string _title = string.Empty;
    private string? _description;

    [JsonConstructor]
    private TaskItem()
    {
    }

    public TaskItem(string title, TaskPriority priority = TaskPriority.Medium, DateTime? dueDate = null, string? description = null)
    {
        Title = title;
        Priority = priority;
        DueDate = dueDate;
        Description = description;
    }

    public string Title
    {
        get => _title;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Название задачи обязательно.", nameof(Title));
            }

            string trimmed = value.Trim();
            if (trimmed.Length > TitleMaxLength)
            {
                throw new ArgumentException($"Название не может быть длиннее {TitleMaxLength} символов.", nameof(Title));
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
                throw new ArgumentException($"Описание не может быть длиннее {DescriptionMaxLength} символов.", nameof(Description));
            }

            _description = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }

    public TaskPriority Priority { get; set; }

    [JsonInclude]
    public TaskItemStatus Status { get; private set; } = TaskItemStatus.Todo;

    public DateTime? DueDate { get; set; }

    [JsonInclude]
    public DateTime? CompletedAt { get; private set; }

    public int? ProjectId { get; set; }

    [JsonIgnore]
    public Project? Project { get; set; }

    public void Start()
    {
        if (Status != TaskItemStatus.Todo)
        {
            throw new InvalidOperationException($"Начать можно только задачу в статусе {TaskItemStatus.Todo}, текущий статус: {Status}.");
        }

        Status = TaskItemStatus.InProgress;
    }

    public void Complete(DateTime now)
    {
        if (Status == TaskItemStatus.Done)
        {
            throw new InvalidOperationException("Задача уже выполнена.");
        }

        Status = TaskItemStatus.Done;
        CompletedAt = now;
    }

    public void Reopen()
    {
        if (Status != TaskItemStatus.Done)
        {
            throw new InvalidOperationException("Вернуть в работу можно только выполненную задачу.");
        }

        Status = TaskItemStatus.Todo;
        CompletedAt = null;
    }

    public bool IsOverdue(DateTime now) =>
        Status != TaskItemStatus.Done
        && DueDate is not null
        && DueDate.Value.Date < now.Date;

    public override string ToString() =>
        $"#{Id} {Title} [{Priority}, {Status}]" + (DueDate is null ? string.Empty : $" срок: {DueDate:yyyy-MM-dd}");
}
