using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Mappings;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Shared.Dtos;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/projects")]
public sealed class ProjectsController(
    IProjectService projectService,
    ITaskService taskService,
    TimeProvider timeProvider,
    ILogger<ProjectsController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProjectDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetAll(CancellationToken cancellationToken)
    {
        var projects = await projectService.GetAllAsync(cancellationToken);
        var result = projects.Select(p => p.ToDto()).ToList();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ProjectDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var project = await projectService.GetByIdAsync(id, cancellationToken);
        if (project is null)
            return NotFound();

        return project.ToDto();
    }

    [HttpPost]
    [ProducesResponseType<ProjectDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProjectDto>> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await projectService.CreateAsync(request.ToEntity(), cancellationToken);
            logger.LogInformation("Project {ProjectId} created", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Project name already exists",
                detail: ex.Message);
        }
    }

    [HttpGet("{id:int}/tasks")]
    [ProducesResponseType<IReadOnlyList<TaskDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> GetTasks(int id, CancellationToken cancellationToken)
    {
        var project = await projectService.GetByIdAsync(id, cancellationToken);
        if (project is null)
            return NotFound();

        var tasks = await taskService.GetAllAsync(new TaskFilter(ProjectId: id), cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var result = tasks.Select(t => t.ToDto(now)).ToList();
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
    int id,
    [FromBody] UpdateProjectRequest request,
    CancellationToken cancellationToken)
    {
        try
        {
            var updated = await projectService.UpdateAsync(id, request.Name, request.Description, cancellationToken);
            if (!updated)
                return NotFound();

            logger.LogInformation("Project {ProjectId} updated", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Project name already exists",
                detail: ex.Message);
        }
    }
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await projectService.DeleteAsync(id, cancellationToken);
            if (!deleted)
                return NotFound();

            logger.LogInformation("Project {ProjectId} deleted", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Project has tasks",
                detail: ex.Message);
        }
    }
}
