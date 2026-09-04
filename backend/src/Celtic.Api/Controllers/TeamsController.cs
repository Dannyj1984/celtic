using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Celtic.Api.DTOs;
using Celtic.Api.Services;

namespace Celtic.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TeamDto>>> GetTeams()
    {
        var teams = await _teamService.GetAllTeamsAsync();
        return Ok(teams);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TeamDto>> GetTeam(Guid id)
    {
        try
        {
            var team = await _teamService.GetTeamByIdAsync(id);
            return Ok(team);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<TeamDto>> CreateTeam([FromBody] CreateTeamRequest request)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can create teams." });

        var team = await _teamService.CreateTeamAsync(request);
        return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TeamDto>> UpdateTeam(Guid id, [FromBody] UpdateTeamRequest request)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can update teams." });

        try
        {
            var team = await _teamService.UpdateTeamAsync(id, request);
            return Ok(team);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTeam(Guid id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can delete teams." });

        await _teamService.DeleteTeamAsync(id);
        return NoContent();
    }
}
