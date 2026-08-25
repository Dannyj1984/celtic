using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Celtic.Api.DTOs;
using Celtic.Api.Services;

namespace Celtic.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<ActionResult<List<EventDto>>> GetEvents()
    {
        var events = await _eventService.GetAllEventsAsync();
        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetEvent(Guid id)
    {
        try
        {
            var e = await _eventService.GetEventByIdAsync(id);
            return Ok(e);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventRequest request)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can create events." });

        var e = await _eventService.CreateEventAsync(request);
        return CreatedAtAction(nameof(GetEvent), new { id = e.Id }, e);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EventDto>> UpdateEvent(Guid id, [FromBody] UpdateEventRequest request)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can update events." });

        try
        {
            var e = await _eventService.UpdateEventAsync(id, request);
            return Ok(e);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/attendance")]
    public async Task<ActionResult<EventDto>> UpdateAttendance(Guid id, [FromBody] UpdateEventAttendanceRequest request)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can update event attendance." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        try
        {
            var updated = await _eventService.UpdateEventAttendanceAsync(id, request.PlayerIds, userId, request.Captain1PlayerId, request.Captain2PlayerId);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can delete events." });

        await _eventService.DeleteEventAsync(id);
        return NoContent();
    }
}
