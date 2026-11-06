using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Mappings;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Shared.Dtos;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/tags")]
public sealed class TagsController(
    ITagService tagService,
    ILogger<TagsController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TagDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TagDto>>> GetAll(CancellationToken cancellationToken)
    {
        var tags = await tagService.GetAllAsync(cancellationToken);
        var result = tags.Select(p => p.ToDto()).ToList();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<TagDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var tag = await tagService.GetByIdAsync(id, cancellationToken);
        if (tag is null)
            return NotFound();

        return tag.ToDto();
    }

    [HttpPost]
    [ProducesResponseType<TagDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TagDto>> Create(
        [FromBody] CreateTagRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await tagService.CreateAsync(request.ToEntity(), cancellationToken);
            logger.LogInformation("Tag {TagId} created", created.Id);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Tag name already exists",
                detail: ex.Message);
        }
    }
}
