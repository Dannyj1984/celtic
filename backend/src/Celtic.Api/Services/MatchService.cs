using Microsoft.EntityFrameworkCore;
using Celtic.Api.Data;
using Celtic.Api.DTOs;
using Celtic.Api.Models;

namespace Celtic.Api.Services;

public class MatchService : IMatchService
{
    private readonly CelticDbContext _db;

    public MatchService(CelticDbContext db)
    {
        _db = db;
    }

    public async Task<List<MatchDto>> GetAllMatchesAsync()
    {
        var matches = await _db.Matches
            .Include(m => m.Season)
            .Include(m => m.PlayerOfTheMatch)
            .OrderByDescending(m => m.Date)
            .ToListAsync();

        return matches.Select(MapToDto).ToList();
    }

    public async Task<MatchDto> GetMatchByIdAsync(Guid id)
    {
        var m = await _db.Matches
            .Include(m => m.Season)
            .Include(m => m.PlayerOfTheMatch)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (m == null) throw new KeyNotFoundException("Match not found");

        return MapToDto(m);
    }

    public async Task<MatchDto> CreateMatchAsync(CreateMatchRequest request)
    {
        // 1. Create the Match
        var match = new Match
        {
            SeasonId = request.SeasonId,
            Date = request.Date,
            Opposition = request.Opposition,
            Location = request.Location,
            IsPublished = false
        };

        _db.Matches.Add(match);

        // 2. Create associated Event
        var ev = new Event
        {
            SeasonId = request.SeasonId,
            Type = "Match",
            DateTime = request.Date,
            Location = request.Location ?? "TBC",
            Notes = request.Notes ?? $"Match vs {request.Opposition}",
            Match = match
        };

        _db.Events.Add(ev);
        
        await _db.SaveChangesAsync();

        // Link back (though EF handles this via navigation)
        match.EventId = ev.Id;
        await _db.SaveChangesAsync();

        return MapToDto(match);
    }

    public async Task<MatchDto> UpdateMatchAsync(Guid id, UpdateMatchRequest request)
    {
        var m = await _db.Matches
            .Include(m => m.Event)
            .Include(m => m.PlayerOfTheMatch)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (m == null) throw new KeyNotFoundException("Match not found");

        m.SeasonId = request.SeasonId;
        m.Date = request.Date;
        m.Opposition = request.Opposition;
        m.Location = request.Location;
        m.GoalsFor = request.GoalsFor;
        m.GoalsAgainst = request.GoalsAgainst;
        m.MatchReport = request.MatchReport;
        m.IsPublished = request.IsPublished;
        m.PlayerOfTheMatchId = request.PlayerOfTheMatchId;

        // Update associated event if it exists
        if (m.Event != null)
        {
            m.Event.SeasonId = request.SeasonId;
            m.Event.DateTime = request.Date;
            m.Event.Location = request.Location ?? "TBC";
        }

        await _db.SaveChangesAsync();

        // Reload to get PlayerOfTheMatch name if it was just set
        if (m.PlayerOfTheMatchId.HasValue && (m.PlayerOfTheMatch == null || m.PlayerOfTheMatch.Id != m.PlayerOfTheMatchId))
        {
             await _db.Entry(m).Reference(x => x.PlayerOfTheMatch).LoadAsync();
        }

        return MapToDto(m);
    }

    public async Task DeleteMatchAsync(Guid id)
    {
        var m = await _db.Matches
            .Include(m => m.Event)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (m != null)
        {
            if (m.Event != null)
            {
                _db.Events.Remove(m.Event);
            }
            _db.Matches.Remove(m);
            await _db.SaveChangesAsync();
        }
    }

    private static MatchDto MapToDto(Match m) => new(
        m.Id,
        m.SeasonId,
        m.Season?.Name,
        m.Date,
        m.Opposition,
        m.Location,
        m.GoalsFor,
        m.GoalsAgainst,
        m.MatchReport,
        m.IsPublished,
        m.Result,
        m.EventId,
        m.PlayerOfTheMatchId,
        m.PlayerOfTheMatch?.FullName
    );
}
