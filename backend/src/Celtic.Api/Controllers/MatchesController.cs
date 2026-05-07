using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Celtic.Api.DTOs;
using Celtic.Api.Services;

namespace Celtic.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchesController : ControllerBase
{
    private readonly IMatchService _matchService;

    public MatchesController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    [HttpGet]
    public async Task<ActionResult<List<MatchDto>>> GetMatches()
    {
        var matches = await _matchService.GetAllMatchesAsync();
        return Ok(matches);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MatchDto>> GetMatch(Guid id)
    {
        try
        {
            var match = await _matchService.GetMatchByIdAsync(id);
            return Ok(match);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<MatchDto>> CreateMatch([FromBody] CreateMatchRequest request)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can create matches." });

        var match = await _matchService.CreateMatchAsync(request);
        return CreatedAtAction(nameof(GetMatch), new { id = match.Id }, match);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MatchDto>> UpdateMatch(Guid id, [FromBody] UpdateMatchRequest request)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can update matches." });

        try
        {
            var match = await _matchService.UpdateMatchAsync(id, request);
            return Ok(match);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMatch(Guid id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can delete matches." });

        await _matchService.DeleteMatchAsync(id);
        return NoContent();
    }
}
