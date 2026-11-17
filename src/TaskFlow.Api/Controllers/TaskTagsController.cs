using Microsoft.AspNetCore.Mvc;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Shared.Dtos;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/tasks/{id:int}/tags")]
public sealed class TaskTagsController(
    ITaskTagService taskTagService,
    ILogger<TaskTagsController> logger) : ControllerBase
{
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetTags(
        int id,
        [FromBody] SetTaskTagsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await taskTagService.SetTagsAsync(id, request.TagIds, cancellationToken);

        if (!result.TaskFound)
            return NotFound();

        if (result.MissingTagIds.Count > 0)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Unknown tags",
                detail: $"Tags not found: {string.Join(", ", result.MissingTagIds)}.");
        }

        logger.LogInformation("Task {TaskId} tags updated", id);
        return NoContent();
    }
}