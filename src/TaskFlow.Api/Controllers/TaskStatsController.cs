using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Mappings;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Shared.Dtos;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/tasks/stats")]
public sealed class TaskStatsController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<TaskStatsDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskStatsDto>> Get(CancellationToken cancellationToken)
    {
        var stats = await taskService.GetStatsAsync(cancellationToken);
        return Ok(stats.ToDto());
    }
}