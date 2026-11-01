using System.Text.Json.Serialization;

namespace TaskFlow.Domain.Entities;

public class Project : BaseEntity
{
    public const int NameMaxLength = 100;

    private string _name = string.Empty;

    [JsonConstructor]
    private Project()
    {
    }

    public Project(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Название проекта обязательно.", nameof(Name));
            }

            string trimmed = value.Trim();
            if (trimmed.Length > NameMaxLength)
            {
                throw new ArgumentException($"Название проекта не может быть длиннее {NameMaxLength} символов.", nameof(Name));
            }

            _name = trimmed;
        }
    }

    public string? Description { get; set; }

    public List<TaskItem> Tasks { get; } = new();
}
