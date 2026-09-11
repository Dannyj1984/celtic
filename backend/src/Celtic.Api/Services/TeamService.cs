using Microsoft.EntityFrameworkCore;
using Celtic.Api.Data;
using Celtic.Api.DTOs;
using Celtic.Api.Models;

namespace Celtic.Api.Services;

public class TeamService : ITeamService
{
    private readonly CelticDbContext _db;

    public TeamService(CelticDbContext db)
    {
        _db = db;
    }

    public async Task<List<TeamDto>> GetAllTeamsAsync()
    {
        var teams = await _db.Teams
            .Include(t => t.Players)
            .Include(t => t.PlayerTeams)
                .ThenInclude(pt => pt.Player)
            .OrderBy(t => t.Name)
            .ToListAsync();

        return teams.Select(MapToDto).ToList();
    }

    public async Task<TeamDto> GetTeamByIdAsync(Guid id)
    {
        var team = await _db.Teams
            .Include(t => t.Players)
            .Include(t => t.PlayerTeams)
                .ThenInclude(pt => pt.Player)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
            throw new KeyNotFoundException($"Team with ID {id} not found.");

        return MapToDto(team);
    }

    public async Task<TeamDto> CreateTeamAsync(CreateTeamRequest request)
    {
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ColorHex = string.IsNullOrWhiteSpace(request.ColorHex) ? "#006837" : request.ColorHex,
            IsActive = true
        };

        _db.Teams.Add(team);
        await _db.SaveChangesAsync();

        return MapToDto(team);
    }

    public async Task<TeamDto> UpdateTeamAsync(Guid id, UpdateTeamRequest request)
    {
        var team = await _db.Teams
            .Include(t => t.Players)
            .Include(t => t.PlayerTeams)
                .ThenInclude(pt => pt.Player)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
            throw new KeyNotFoundException($"Team with ID {id} not found.");

        team.Name = request.Name;
        team.ColorHex = string.IsNullOrWhiteSpace(request.ColorHex) ? "#006837" : request.ColorHex;
        team.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        return MapToDto(team);
    }

    public async Task DeleteTeamAsync(Guid id)
    {
        var team = await _db.Teams.FindAsync(id);
        if (team != null)
        {
            // Unlink players, matches, events
            var playerTeams = await _db.PlayerTeams.Where(pt => pt.TeamId == id).ToListAsync();
            _db.PlayerTeams.RemoveRange(playerTeams);

            var players = await _db.Players.Where(p => p.TeamId == id).ToListAsync();
            foreach (var p in players) p.TeamId = null;

            var matches = await _db.Matches.Where(m => m.TeamId == id).ToListAsync();
            foreach (var m in matches) m.TeamId = null;

            var events = await _db.Events.Where(e => e.TeamId == id).ToListAsync();
            foreach (var e in events) e.TeamId = null;

            _db.Teams.Remove(team);
            await _db.SaveChangesAsync();
        }
    }

    private static TeamDto MapToDto(Team t)
    {
        var playerTeamPlayerIds = t.PlayerTeams?
            .Where(pt => pt.Player != null && pt.Player.IsActive)
            .Select(pt => pt.PlayerId) ?? Enumerable.Empty<Guid>();

        var legacyPlayerIds = t.Players?
            .Where(p => p.IsActive)
            .Select(p => p.Id) ?? Enumerable.Empty<Guid>();

        var activePlayerCount = playerTeamPlayerIds.Union(legacyPlayerIds).Count();

        return new TeamDto(
            t.Id,
            t.Name,
            t.ColorHex,
            t.IsActive,
            activePlayerCount
        );
    }
}
