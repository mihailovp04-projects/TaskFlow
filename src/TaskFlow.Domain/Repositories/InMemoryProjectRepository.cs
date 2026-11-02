using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

public sealed class InMemoryProjectRepository : InMemoryRepository<Project>, IProjectRepository
{
}