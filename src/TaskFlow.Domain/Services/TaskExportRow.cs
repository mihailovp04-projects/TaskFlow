namespace TaskFlow.Domain.Services;

public sealed class TaskExportRow
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Priority { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string? DueDate { get; init; }
}