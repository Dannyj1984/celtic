using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Celtic.Api.DTOs;
using Celtic.Api.Services;

namespace Celtic.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All season endpoints require authentication
public class SeasonsController : ControllerBase
{
    private readonly ISeasonService _seasonService;

    public SeasonsController(ISeasonService seasonService)
    {
        _seasonService = seasonService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SeasonDto>>> GetSeasons()
    {
        var seasons = await _seasonService.GetAllSeasonsAsync();
        return Ok(seasons);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SeasonDto>> GetSeason(Guid id)
    {
        try
        {
            var season = await _seasonService.GetSeasonByIdAsync(id);
            return Ok(season);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<SeasonDto>> CreateSeason([FromBody] CreateSeasonRequest request)
    {
        // Only admins can create seasons
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can create seasons." });

        var season = await _seasonService.CreateSeasonAsync(request);
        return CreatedAtAction(nameof(GetSeason), new { id = season.Id }, season);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SeasonDto>> UpdateSeason(Guid id, [FromBody] UpdateSeasonRequest request)
    {
        // Only admins can update seasons
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can update seasons." });

        try
        {
            var season = await _seasonService.UpdateSeasonAsync(id, request);
            return Ok(season);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
