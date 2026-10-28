namespace TaskFlow.Domain.Entities;

public class Project
{
    public const int NameMaxLength = 100;

    private string _name = string.Empty;

    public Project(string name, string? description = null)
    {
        Name = name;
        Description = description;
        CreatedAt = DateTime.Now;
    }

    public int Id { get; set; }

    public DateTime CreatedAt { get; private set; }

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Project name is required.", nameof(Name));
            }

            var trimmed = value.Trim();
            if (trimmed.Length > NameMaxLength)
            {
                throw new ArgumentException($"Project name must be at most {NameMaxLength} characters.", nameof(Name));
            }

            _name = trimmed;
        }
    }

    public string? Description
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public List<TaskItem> Tasks { get; } = new();

    public void AddTask(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (Tasks.Contains(task))
        {
            throw new InvalidOperationException($"Task #{task.Id} is already in project '{Name}'.");
        }

        Tasks.Add(task);
        task.Project = this;
        task.ProjectId = Id;
    }

    public int CountOverdue(DateTime now)
    {
        var count = 0;
        foreach (var task in Tasks)
        {
            if (task.IsOverdue(now))
            {
                count++;
            }
        }

        return count;
    }
}
