using TaskFlow.Domain.Entities;
using TaskFlow.Shared.Dtos;

namespace TaskFlow.Api.Mappings;

public static class TagMappings
{
    public static TagDto ToDto(this Tag tag) =>
        new(tag.Id, tag.Name);

    public static Tag ToEntity(this CreateTagRequest request) =>
        new(request.Name);
}
