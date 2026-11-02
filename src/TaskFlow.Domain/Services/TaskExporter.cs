using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Services;

public sealed class TaskExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public string Export(IEnumerable<TaskItem> tasks)
    {
        List<TaskExportRow> rows = tasks
            .Select(t => new TaskExportRow
            {
                Id = t.Id,
                Title = t.Title,
                Priority = t.Priority.ToString(),
                Status = t.Status.ToString(),
                DueDate = t.DueDate?.ToString("yyyy-MM-dd")
            })
            .ToList();

        return JsonSerializer.Serialize(rows, JsonOptions);
    }
}