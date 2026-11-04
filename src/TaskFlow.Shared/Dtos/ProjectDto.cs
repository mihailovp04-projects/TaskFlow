namespace TaskFlow.Shared.Dtos;

public sealed record ProjectDto(int Id, string Name, string? Description, DateTime CreatedAt);
