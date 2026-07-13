using MediatR;
using Microsoft.AspNetCore.Mvc;
using EventRegistration.Api.Features.Registrations;

namespace EventRegistration.Api.Controllers;

[ApiController]
[Route("api/events/{eventId}/registrations")]
public class RegistrationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RegistrationsController> _logger;

    public RegistrationsController(IMediator mediator, ILogger<RegistrationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of registrations for an event
    /// </summary>
    /// <param name="eventId">Event ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
    /// <param name="search">Search by participant name, email, or phone</param>
    /// <param name="status">Filter by status (1=Active, 2=Cancelled)</param>
    /// <returns>Paginated list of registrations</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<RegistrationListItem>>> GetEventRegistrations(
        long eventId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? status = null)
    {
        var query = new GetEventRegistrationsQuery
        {
            EventId = eventId,
            Page = page,
            PageSize = pageSize,
            Search = search,
            Status = status
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Register or reactivate a participant in an event
    /// </summary>
    /// <param name="eventId">Event ID</param>
    /// <param name="request">Registration data with participant ID and optional notes</param>
    /// <returns>Created registration</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegistrationResponse>> RegisterParticipant(
        long eventId,
        [FromBody] RegisterParticipantRequest request)
    {
        var command = new RegisterParticipantCommand
        {
            EventId = eventId,
            ParticipantId = request.ParticipantId,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(
            nameof(GetRegistrationById),
            new { eventId = eventId, registrationId = result.Id },
            result);
    }

    /// <summary>
    /// Get a single registration by ID
    /// </summary>
    /// <param name="eventId">Event ID</param>
    /// <param name="registrationId">Registration ID</param>
    /// <returns>Registration details</returns>
    [HttpGet("{registrationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegistrationResponse>> GetRegistrationById(long eventId, long registrationId)
    {
        var query = new GetRegistrationByIdQuery { RegistrationId = registrationId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Cancel an active registration
    /// </summary>
    /// <param name="eventId">Event ID</param>
    /// <param name="registrationId">Registration ID</param>
    /// <returns>Updated registration</returns>
    [HttpPatch("{registrationId}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegistrationResponse>> CancelRegistration(long eventId, long registrationId)
    {
        var command = new CancelRegistrationCommand { RegistrationId = registrationId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

public class RegisterParticipantRequest
{
    public long ParticipantId { get; set; }
    public string? Notes { get; set; }
}