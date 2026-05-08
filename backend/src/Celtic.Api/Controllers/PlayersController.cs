using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Celtic.Api.DTOs;
using Celtic.Api.Services;

namespace Celtic.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All player endpoints require authentication
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;

    public PlayersController(IPlayerService playerService)
    {
        _playerService = playerService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PlayerDto>>> GetPlayers()
    {
        var players = await _playerService.GetAllPlayersAsync();
        return Ok(players);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerDto>> GetPlayer(Guid id)
    {
        try
        {
            var player = await _playerService.GetPlayerByIdAsync(id);
            return Ok(player);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<PlayerDto>> CreatePlayer([FromBody] CreatePlayerRequest request)
    {
        // Only admins can create players
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can create players." });

        var player = await _playerService.CreatePlayerAsync(request);
        return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PlayerDto>> UpdatePlayer(Guid id, [FromBody] UpdatePlayerRequest request)
    {
        // Only admins can update players
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can update players." });

        try
        {
            var player = await _playerService.UpdatePlayerAsync(id, request);
            return Ok(player);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
    [HttpPatch("{id}/subscription-status")]
    public async Task<ActionResult<PlayerDto>> UpdateSubscriptionStatus(Guid id, [FromBody] UpdateSubscriptionStatusRequest request)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can update subscription status." });

        var validStatuses = new[] { "Active", "Payment Due", "Inactive" };
        if (!validStatuses.Contains(request.SubscriptionStatus))
            return BadRequest(new { message = "Invalid subscription status." });

        try
        {
            var player = await _playerService.UpdateSubscriptionStatusAsync(id, request.SubscriptionStatus);
            return Ok(player);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
