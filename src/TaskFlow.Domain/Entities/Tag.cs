using System.Text.Json.Serialization;

namespace TaskFlow.Domain.Entities;

public class Tag : BaseEntity
{
    public const int NameMaxLength = 50;

    private string _name = string.Empty;

    [JsonConstructor]
    private Tag()
    {
    }

    public Tag(string name)
    {
        Name = name;
    }

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Название метки обязательно.", nameof(Name));
            }

            string trimmed = value.Trim();
            if (trimmed.Length > NameMaxLength)
            {
                throw new ArgumentException($"Название метки не может быть длиннее {NameMaxLength} символов.", nameof(Name));
            }

            _name = trimmed;
        }
    }

    public List<TaskItem> Tasks { get; } = new();
}
