using Microsoft.EntityFrameworkCore;
using Celtic.Api.Data;
using Celtic.Api.DTOs;
using Celtic.Api.Models;

namespace Celtic.Api.Services;

public class EventService : IEventService
{
    private readonly CelticDbContext _db;

    public EventService(CelticDbContext db)
    {
        _db = db;
    }

    public async Task<List<EventDto>> GetAllEventsAsync()
    {
        var events = await _db.Events
            .Include(e => e.Season)
            .Include(e => e.Responses)
                .ThenInclude(r => r.Player)
            .OrderBy(e => e.DateTime)
            .ToListAsync();

        return events.Select(MapToDto).ToList();
    }

    public async Task<EventDto> GetEventByIdAsync(Guid id)
    {
        var e = await _db.Events
            .Include(e => e.Season)
            .Include(e => e.Responses)
                .ThenInclude(r => r.Player)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (e == null) throw new KeyNotFoundException("Event not found");

        return MapToDto(e);
    }

    public async Task<EventDto> CreateEventAsync(CreateEventRequest request)
    {
        var e = new Event
        {
            SeasonId = request.SeasonId,
            Type = request.Type,
            DateTime = request.DateTime,
            Location = request.Location,
            Notes = request.Notes
        };

        _db.Events.Add(e);
        await _db.SaveChangesAsync();

        return MapToDto(e);
    }

    public async Task<EventDto> UpdateEventAsync(Guid id, UpdateEventRequest request)
    {
        var e = await _db.Events.FindAsync(id);
        if (e == null) throw new KeyNotFoundException("Event not found");

        e.DateTime = request.DateTime;
        e.Location = request.Location;
        e.Notes = request.Notes;
        e.IsCancelled = request.IsCancelled;

        await _db.SaveChangesAsync();
        return MapToDto(e);
    }

    public async Task DeleteEventAsync(Guid id)
    {
        var e = await _db.Events.FindAsync(id);
        if (e != null)
        {
            _db.Events.Remove(e);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<EventDto> UpdateEventAttendanceAsync(Guid eventId, List<Guid> playerIds, string adminUserId)
    {
        var e = await _db.Events.FindAsync(eventId);
        if (e == null) throw new KeyNotFoundException("Event not found");

        var validUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == adminUserId);
        var effectiveUserId = validUser?.Id ?? (await _db.Users.Select(u => u.Id).FirstOrDefaultAsync()) ?? adminUserId;

        var existingResponses = await _db.EventResponses
            .Where(r => r.EventId == eventId)
            .ToListAsync();

        var targetPlayerIds = playerIds.Distinct().ToHashSet();

        foreach (var resp in existingResponses)
        {
            if (targetPlayerIds.Contains(resp.PlayerId))
            {
                resp.Status = "Attending";
                if (!string.IsNullOrEmpty(effectiveUserId))
                    resp.RespondedByUserId = effectiveUserId;
                resp.RespondedAt = DateTime.UtcNow;
            }
            else if (resp.Status == "Attending")
            {
                resp.Status = "NotAttending";
                if (!string.IsNullOrEmpty(effectiveUserId))
                    resp.RespondedByUserId = effectiveUserId;
                resp.RespondedAt = DateTime.UtcNow;
            }
        }

        var existingPlayerIds = existingResponses.Select(r => r.PlayerId).ToHashSet();
        var missingPlayerIds = targetPlayerIds.Where(id => !existingPlayerIds.Contains(id)).ToList();

        foreach (var playerId in missingPlayerIds)
        {
            _db.EventResponses.Add(new EventResponse
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                PlayerId = playerId,
                Status = "Attending",
                RespondedByUserId = effectiveUserId,
                RespondedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        return await GetEventByIdAsync(eventId);
    }

    private static EventDto MapToDto(Event e) => new(
        e.Id,
        e.SeasonId,
        e.Season?.Name,
        e.Type,
        e.DateTime,
        e.Location,
        e.Notes,
        e.IsCancelled,
        e.MatchId,
        e.Responses
            .Where(r => r.Status == "Attending" && r.Player != null)
            .Select(r => new AttendingPlayerDto(r.PlayerId, r.Player.FullName))
            .ToList()
    );
}
