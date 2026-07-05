using MediatR;
using Microsoft.AspNetCore.Mvc;
using EventRegistration.Api.Features.Participants;

namespace EventRegistration.Api.Controllers;

/// <summary>
/// Participants API Controller
/// Routes: GET /api/participants, GET /api/participants/{id}, POST /api/participants, PUT /api/participants/{id}, DELETE /api/participants/{id}
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ParticipantsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ParticipantsController> _logger;

    public ParticipantsController(IMediator mediator, ILogger<ParticipantsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of participants with search and filtering
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
    /// <param name="search">Search by name, email, or phone</param>
    /// <param name="isActive">Filter by active state</param>
    /// <returns>Paginated list of participants</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResult<ParticipantListItem>>> GetParticipants(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetParticipantsQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            IsActive = isActive
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a single participant by ID
    /// </summary>
    /// <param name="id">Participant ID</param>
    /// <returns>Participant details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParticipantResponse>> GetParticipantById(long id)
    {
        var query = new GetParticipantByIdQuery { ParticipantId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new participant
    /// </summary>
    /// <param name="request">Participant creation data</param>
    /// <returns>Created participant</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ParticipantResponse>> CreateParticipant([FromBody] CreateParticipantCommand request)
    {
        var result = await _mediator.Send(request);
        return CreatedAtAction(nameof(GetParticipantById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing participant
    /// </summary>
    /// <param name="id">Participant ID</param>
    /// <param name="request">Participant update data</param>
    /// <returns>Updated participant</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ParticipantResponse>> UpdateParticipant(long id, [FromBody] UpdateParticipantCommand request)
    {
        request.ParticipantId = id;
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    /// <summary>
    /// Delete a participant (only if no registration history)
    /// </summary>
    /// <param name="id">Participant ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteParticipant(long id)
    {
        var command = new DeleteParticipantCommand { ParticipantId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
