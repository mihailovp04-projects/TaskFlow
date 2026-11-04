using TaskFlow.Domain.Entities;
using TaskFlow.Shared.Dtos;

namespace TaskFlow.Api.Mappings;

public static class ProjectMappings
{
    public static ProjectDto ToDto(this Project project) =>
        new(project.Id, project.Name, project.Description, project.CreatedAt);

    public static Project ToEntity(this CreateProjectRequest request) =>
        new(request.Name, request.Description);
}
