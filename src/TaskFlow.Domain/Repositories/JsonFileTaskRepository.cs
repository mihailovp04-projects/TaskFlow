using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

public sealed class JsonFileTaskRepository : ITaskRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _filePath;
    private readonly InMemoryTaskRepository _inner;

    public JsonFileTaskRepository(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Путь к файлу обязателен.", nameof(filePath));
        }

        _filePath = filePath;
        _inner = new InMemoryTaskRepository(Load());
    }

    public IReadOnlyList<TaskItem> GetAll() => _inner.GetAll();

    public TaskItem? GetById(int id) => _inner.GetById(id);

    public void Add(TaskItem task)
    {
        _inner.Add(task);
        Save();
    }

    public void Update(TaskItem task)
    {
        _inner.Update(task);
        Save();
    }

    public bool Delete(int id)
    {
        bool deleted = _inner.Delete(id);
        if (deleted)
        {
            Save();
        }

        return deleted;
    }

    private List<TaskItem> Load()
    {
        if (!File.Exists(_filePath))
        {
            return new List<TaskItem>();
        }

        string json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<TaskItem>();
        }

        return JsonSerializer.Deserialize<List<TaskItem>>(json, JsonOptions) ?? new List<TaskItem>();
    }

    private void Save()
    {
        string? directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(_inner.GetAll(), JsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
