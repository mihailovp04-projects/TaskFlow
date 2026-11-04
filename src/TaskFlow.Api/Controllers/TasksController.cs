using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Mappings;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Enums;
using TaskFlow.Shared.Dtos;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController(
    ITaskService taskService,
    TimeProvider timeProvider,
    ILogger<TasksController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TaskDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> GetAll(
        TaskItemStatus? status,
        TaskPriority? priority,
        int? projectId,
        string? search,
        CancellationToken cancellationToken)
    {
        var filter = new TaskFilter(status, priority, projectId, search);
        var tasks = await taskService.GetAllAsync(filter, cancellationToken);

        var now = Now();
        var result = tasks.Select(t => t.ToDto(now)).ToList();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<TaskDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var task = await taskService.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return NotFound();

        return task.ToDto(Now());
    }

    [HttpPost]
    [ProducesResponseType<TaskDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var created = await taskService.CreateAsync(request.ToEntity(), cancellationToken);
        logger.LogInformation("Task {TaskId} created with priority {Priority}", created.Id, created.Priority);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto(Now()));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await taskService.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return NotFound();

        request.ApplyTo(task);
        await taskService.UpdateAsync(task, cancellationToken);
        logger.LogInformation("Task {TaskId} updated", id);

        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeStatus(
        int id,
        [FromBody] ChangeStatusRequest request,
        CancellationToken cancellationToken)
    {
        var newStatus = request.Status!.Value;

        try
        {
            var task = await taskService.ChangeStatusAsync(id, newStatus, cancellationToken);
            if (task is null)
                return NotFound();

            logger.LogInformation("Task {TaskId} moved to status {Status}", id, task.Status);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Invalid status transition for task {TaskId} to {Status}: {Reason}",
                id, newStatus, ex.Message);
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Invalid status transition",
                detail: ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await taskService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound();

        logger.LogInformation("Task {TaskId} deleted", id);
        return NoContent();
    }

    private DateTime Now() => timeProvider.GetUtcNow().UtcDateTime;
}
