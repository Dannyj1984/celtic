using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Celtic.Api.DTOs;
using Celtic.Api.Services;

namespace Celtic.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class MatchSquadsController : ControllerBase
{
    private readonly IMatchSquadService _squadService;

    public MatchSquadsController(IMatchSquadService squadService)
    {
        _squadService = squadService;
    }

    [HttpGet("matches/{matchId}/squad")]
    public async Task<ActionResult<MatchSquadDto>> GetSquadByMatch(Guid matchId)
    {
        var squad = await _squadService.GetSquadByMatchIdAsync(matchId);
        if (squad == null)
            return NotFound(new { message = "No squad plan found for this match." });

        return Ok(squad);
    }

    [HttpPost("matches/{matchId}/squad/generate")]
    public async Task<ActionResult<MatchSquadDto>> GenerateSquadForMatch(Guid matchId, [FromBody] GenerateMatchSquadRequest? request)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can generate match squads." });

        var req = (request ?? new GenerateMatchSquadRequest()) with { MatchId = matchId };
        try
        {
            var squad = await _squadService.GenerateSquadAsync(req);
            return Ok(squad);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("matches/{matchId}/squad")]
    public async Task<ActionResult<MatchSquadDto>> SaveSquadForMatch(Guid matchId, [FromBody] SaveMatchSquadRequest request)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can save match squads." });

        var squad = await _squadService.SaveSquadAsync(matchId, request.EventId, request);
        return Ok(squad);
    }

    [HttpGet("events/{eventId}/squad")]
    public async Task<ActionResult<MatchSquadDto>> GetSquadByEvent(Guid eventId)
    {
        var squad = await _squadService.GetSquadByEventIdAsync(eventId);
        if (squad == null)
            return NotFound(new { message = "No squad plan found for this event." });

        return Ok(squad);
    }

    [HttpPost("events/{eventId}/squad/generate")]
    public async Task<ActionResult<MatchSquadDto>> GenerateSquadForEvent(Guid eventId, [FromBody] GenerateMatchSquadRequest? request)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can generate match squads." });

        var req = (request ?? new GenerateMatchSquadRequest()) with { EventId = eventId };
        try
        {
            var squad = await _squadService.GenerateSquadAsync(req);
            return Ok(squad);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("events/{eventId}/squad")]
    public async Task<ActionResult<MatchSquadDto>> SaveSquadForEvent(Guid eventId, [FromBody] SaveMatchSquadRequest request)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can save match squads." });

        var squad = await _squadService.SaveSquadAsync(request.MatchId, eventId, request);
        return Ok(squad);
    }
}
