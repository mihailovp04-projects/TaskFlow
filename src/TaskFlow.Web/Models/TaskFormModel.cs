using System.ComponentModel.DataAnnotations;
using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Dtos;

namespace TaskFlow.Web.Models;

// Модель формы задачи: одна для создания и для редактирования
public sealed class TaskFormModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title must be at most 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description must be at most 2000 characters.")]
    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime? DueDate { get; set; }

    public int? ProjectId { get; set; }

    public static TaskFormModel FromDto(TaskDto dto) => new()
    {
        Title = dto.Title,
        Description = dto.Description,
        Priority = dto.Priority,
        DueDate = dto.DueDate,
        ProjectId = dto.ProjectId
    };

    public CreateTaskRequest ToCreateRequest() => new()
    {
        Title = Title.Trim(),
        Description = NormalizeText(Description),
        Priority = Priority,
        DueDate = DueDate,
        ProjectId = ProjectId
    };

    public UpdateTaskRequest ToUpdateRequest() => new()
    {
        Title = Title.Trim(),
        Description = NormalizeText(Description),
        Priority = Priority,
        DueDate = DueDate,
        ProjectId = ProjectId
    };

    // пустое описание отправляем как null, а не как "   "
    private static string? NormalizeText(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
