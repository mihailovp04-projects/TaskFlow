using TaskFlow.Domain.Entities;
using TaskFlow.Shared.Dtos;

namespace TaskFlow.Api.Mappings;

public static class ProjectMappings
{
    public static ProjectDto ToDto(this Project project) => new()
    {
        Id = project.Id,
        Name = project.Name,
        Description = project.Description,
        CreatedAt = project.CreatedAt,
        Color = project.Color
    };


    public static Project ToEntity(this CreateProjectRequest request) =>
        new(request.Name, request.Description)
        {
            Color = request.Color
        };
}
