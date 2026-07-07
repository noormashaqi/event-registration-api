using EventRegistration.Api.Features.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventRegistration.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventsController(IMediator mediator) 
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents([FromQuery] GetEvents.Query query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(long id)
        {
            var result = await _mediator.Send(new GetEventById.Query { Id = id });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEvent.Command command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetEventById), new { id }, new { id });
        }

        // // some random testing data update 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(long id, [FromBody] UpdateEvent.Command command)
        {
            command.Id = id;
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(long id)
        {
            await _mediator.Send(new DeleteEvent.Command { Id = id });
            return NoContent();
        }
    }
}