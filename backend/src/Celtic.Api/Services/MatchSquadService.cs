using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Celtic.Api.Data;
using Celtic.Api.DTOs;
using Celtic.Api.Models;

namespace Celtic.Api.Services;

public class MatchSquadService : IMatchSquadService
{
    private readonly CelticDbContext _context;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public MatchSquadService(CelticDbContext context)
    {
        _context = context;
    }

    public async Task<MatchSquadDto> GenerateSquadAsync(GenerateMatchSquadRequest request)
    {
        var registeredPlayers = await GetEligiblePlayersAsync(request);

        if (registeredPlayers.Count == 0)
        {
            throw new InvalidOperationException("No registered players found for this match.");
        }

        // Determine half duration and format
        int defaultHalfDuration = 18;
        string defaultFormat = "5v5";
        if (request.MatchId.HasValue)
        {
            var match = await _context.Matches.FirstOrDefaultAsync(m => m.Id == request.MatchId.Value);
            if (match != null)
            {
                if (match.HalfDurationMinutes > 0)
                {
                    defaultHalfDuration = match.HalfDurationMinutes;
                }
                if (!string.IsNullOrWhiteSpace(match.Format))
                {
                    defaultFormat = match.Format;
                }
            }
        }
        else if (request.EventId.HasValue)
        {
            var evt = await _context.Events.Include(e => e.Match).FirstOrDefaultAsync(e => e.Id == request.EventId.Value);
            if (evt?.Match != null)
            {
                if (evt.Match.HalfDurationMinutes > 0)
                {
                    defaultHalfDuration = evt.Match.HalfDurationMinutes;
                }
                if (!string.IsNullOrWhiteSpace(evt.Match.Format))
                {
                    defaultFormat = evt.Match.Format;
                }
            }
        }

        var halfDuration = request.HalfDurationMinutes.HasValue && request.HalfDurationMinutes.Value > 0
            ? request.HalfDurationMinutes.Value
            : defaultHalfDuration;

        var format = !string.IsNullOrWhiteSpace(request.Format) ? request.Format : defaultFormat;
        var is3v3 = string.Equals(format, "3v3", StringComparison.OrdinalIgnoreCase);

        var intervals = (request.TotalPeriods.HasValue && request.TotalPeriods.Value > 0 && request.PeriodDurationMinutes.HasValue && request.PeriodDurationMinutes.Value > 0)
            ? null
            : BuildPeriodIntervals(halfDuration);

        var totalPeriods = intervals != null ? intervals.Count : request.TotalPeriods!.Value;
        var periodMinutes = intervals != null ? (halfDuration == 15 ? 5 : 6) : request.PeriodDurationMinutes!.Value;

        SquadPlayerDto? gk1 = null;
        SquadPlayerDto? gk2 = null;

        if (!is3v3)
        {
            // Pick Goalkeepers for 5v5
            gk1 = registeredPlayers.FirstOrDefault(p => p.Id == request.FirstHalfGoalkeeperPlayerId)
                  ?? registeredPlayers.FirstOrDefault();

            gk2 = registeredPlayers.FirstOrDefault(p => p.Id == request.SecondHalfGoalkeeperPlayerId)
                  ?? (registeredPlayers.Count > 1 ? registeredPlayers.First(p => p.Id != gk1?.Id) : gk1);
        }

        var periods = GeneratePeriods(registeredPlayers, gk1, gk2, halfDuration, format, request.TotalPeriods, request.PeriodDurationMinutes);
        var playerMinutes = CalculateMinutes(registeredPlayers, periods, periodMinutes);

        var squadDto = new MatchSquadDto(
            Id: Guid.NewGuid(),
            MatchId: request.MatchId,
            EventId: request.EventId,
            HalfDurationMinutes: halfDuration,
            Format: format,
            TotalPeriods: totalPeriods,
            PeriodDurationMinutes: periodMinutes,
            FirstHalfGoalkeeperPlayerId: gk1?.Id,
            FirstHalfGoalkeeperName: gk1?.Name,
            SecondHalfGoalkeeperPlayerId: gk2?.Id,
            SecondHalfGoalkeeperName: gk2?.Name,
            RegisteredPlayers: registeredPlayers,
            Periods: periods,
            PlayerMinutes: playerMinutes,
            UpdatedAt: DateTime.UtcNow
        );

        return squadDto;
    }

    public async Task<MatchSquadDto?> GetSquadByMatchIdAsync(Guid matchId)
    {
        var squad = await _context.MatchSquads
            .Include(ms => ms.FirstHalfGoalkeeperPlayer)
            .Include(ms => ms.SecondHalfGoalkeeperPlayer)
            .FirstOrDefaultAsync(ms => ms.MatchId == matchId);

        if (squad == null)
        {
            // Also check if match has an event
            var match = await _context.Matches.Include(m => m.Event).FirstOrDefaultAsync(m => m.Id == matchId);
            if (match?.EventId != null)
            {
                squad = await _context.MatchSquads
                    .Include(ms => ms.FirstHalfGoalkeeperPlayer)
                    .Include(ms => ms.SecondHalfGoalkeeperPlayer)
                    .FirstOrDefaultAsync(ms => ms.EventId == match.EventId);
            }
        }

        return squad == null ? null : MapToDto(squad);
    }

    public async Task<MatchSquadDto?> GetSquadByEventIdAsync(Guid eventId)
    {
        var squad = await _context.MatchSquads
            .Include(ms => ms.FirstHalfGoalkeeperPlayer)
            .Include(ms => ms.SecondHalfGoalkeeperPlayer)
            .FirstOrDefaultAsync(ms => ms.EventId == eventId);

        if (squad == null)
        {
            var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            if (evt?.MatchId != null)
            {
                squad = await _context.MatchSquads
                    .Include(ms => ms.FirstHalfGoalkeeperPlayer)
                    .Include(ms => ms.SecondHalfGoalkeeperPlayer)
                    .FirstOrDefaultAsync(ms => ms.MatchId == evt.MatchId);
            }
        }

        return squad == null ? null : MapToDto(squad);
    }

    public async Task<MatchSquadDto> SaveSquadAsync(Guid? matchId, Guid? eventId, SaveMatchSquadRequest request)
    {
        MatchSquad? squad = null;
        if (matchId.HasValue)
        {
            squad = await _context.MatchSquads.FirstOrDefaultAsync(ms => ms.MatchId == matchId.Value);
        }
        if (squad == null && eventId.HasValue)
        {
            squad = await _context.MatchSquads.FirstOrDefaultAsync(ms => ms.EventId == eventId.Value);
        }

        var periods = request.Periods;
        var periodMinutes = request.PeriodDurationMinutes > 0 ? request.PeriodDurationMinutes : 6;
        var totalPeriods = request.TotalPeriods > 0 ? request.TotalPeriods : periods.Count;

        // Collect all distinct players from periods
        var registeredPlayers = request.RegisteredPlayers;
        if (registeredPlayers == null || registeredPlayers.Count == 0)
        {
            var dict = new Dictionary<Guid, SquadPlayerDto>();
            foreach (var p in periods)
            {
                if (p.Goalkeeper != null) dict[p.Goalkeeper.Id] = p.Goalkeeper;
                foreach (var of in p.OutfieldPlayers) dict[of.Id] = of;
                foreach (var b in p.BenchPlayers) dict[b.Id] = b;
            }
            registeredPlayers = dict.Values.ToList();
        }

        // Recompute substitutions between consecutive periods to ensure accuracy
        for (int i = 1; i < periods.Count; i++)
        {
            var prevPeriod = periods[i - 1];
            var currentPeriod = periods[i];

            var prevOnPitch = new HashSet<Guid>();
            if (prevPeriod.Goalkeeper != null) prevOnPitch.Add(prevPeriod.Goalkeeper.Id);
            foreach (var of in prevPeriod.OutfieldPlayers) prevOnPitch.Add(of.Id);

            var currOnPitch = new HashSet<Guid>();
            if (currentPeriod.Goalkeeper != null) currOnPitch.Add(currentPeriod.Goalkeeper.Id);
            foreach (var of in currentPeriod.OutfieldPlayers) currOnPitch.Add(of.Id);

            var comingOn = currentPeriod.OutfieldPlayers
                .Concat(currentPeriod.Goalkeeper != null ? new[] { currentPeriod.Goalkeeper } : Enumerable.Empty<SquadPlayerDto>())
                .Where(p => !prevOnPitch.Contains(p.Id))
                .ToList();

            var goingOff = prevPeriod.OutfieldPlayers
                .Concat(prevPeriod.Goalkeeper != null ? new[] { prevPeriod.Goalkeeper } : Enumerable.Empty<SquadPlayerDto>())
                .Where(p => !currOnPitch.Contains(p.Id))
                .ToList();

            var subs = new List<SubstitutionInfoDto>();
            var subCount = Math.Min(comingOn.Count, goingOff.Count);
            for (int s = 0; s < subCount; s++)
            {
                subs.Add(new SubstitutionInfoDto(
                    PlayerInId: comingOn[s].Id,
                    PlayerInName: comingOn[s].Name,
                    PlayerOutId: goingOff[s].Id,
                    PlayerOutName: goingOff[s].Name
                ));
            }

            periods[i] = currentPeriod with { Substitutions = subs };
        }

        var playerMinutes = CalculateMinutes(registeredPlayers, periods, periodMinutes);

        var payload = new SavedSquadPayload(
            RegisteredPlayers: registeredPlayers,
            Periods: periods,
            PlayerMinutes: playerMinutes
        );

        var json = JsonSerializer.Serialize(payload, JsonOptions);

        var halfDuration = request.HalfDurationMinutes > 0 ? request.HalfDurationMinutes : (totalPeriods * periodMinutes) / 2;
        var format = !string.IsNullOrWhiteSpace(request.Format) ? request.Format : "5v5";

        if (squad == null)
        {
            squad = new MatchSquad
            {
                Id = Guid.NewGuid(),
                MatchId = matchId,
                EventId = eventId,
                HalfDurationMinutes = halfDuration,
                Format = format,
                TotalPeriods = totalPeriods,
                PeriodDurationMinutes = periodMinutes,
                FirstHalfGoalkeeperPlayerId = request.FirstHalfGoalkeeperPlayerId,
                SecondHalfGoalkeeperPlayerId = request.SecondHalfGoalkeeperPlayerId,
                SquadDataJson = json,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.MatchSquads.Add(squad);
        }
        else
        {
            squad.MatchId = matchId ?? squad.MatchId;
            squad.EventId = eventId ?? squad.EventId;
            squad.HalfDurationMinutes = halfDuration;
            squad.Format = format;
            squad.TotalPeriods = totalPeriods;
            squad.PeriodDurationMinutes = periodMinutes;
            squad.FirstHalfGoalkeeperPlayerId = request.FirstHalfGoalkeeperPlayerId;
            squad.SecondHalfGoalkeeperPlayerId = request.SecondHalfGoalkeeperPlayerId;
            squad.SquadDataJson = json;
            squad.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Load keeper names
        string? gk1Name = null;
        if (request.FirstHalfGoalkeeperPlayerId.HasValue)
        {
            gk1Name = (await _context.Players.FindAsync(request.FirstHalfGoalkeeperPlayerId.Value))?.FullName;
        }
        string? gk2Name = null;
        if (request.SecondHalfGoalkeeperPlayerId.HasValue)
        {
            gk2Name = (await _context.Players.FindAsync(request.SecondHalfGoalkeeperPlayerId.Value))?.FullName;
        }

        return new MatchSquadDto(
            Id: squad.Id,
            MatchId: squad.MatchId,
            EventId: squad.EventId,
            HalfDurationMinutes: squad.HalfDurationMinutes,
            Format: squad.Format,
            TotalPeriods: totalPeriods,
            PeriodDurationMinutes: periodMinutes,
            FirstHalfGoalkeeperPlayerId: squad.FirstHalfGoalkeeperPlayerId,
            FirstHalfGoalkeeperName: gk1Name,
            SecondHalfGoalkeeperPlayerId: squad.SecondHalfGoalkeeperPlayerId,
            SecondHalfGoalkeeperName: gk2Name,
            RegisteredPlayers: registeredPlayers,
            Periods: periods,
            PlayerMinutes: playerMinutes,
            UpdatedAt: squad.UpdatedAt
        );
    }

    private async Task<List<SquadPlayerDto>> GetEligiblePlayersAsync(GenerateMatchSquadRequest request)
    {
        if (request.CustomPlayerIds != null && request.CustomPlayerIds.Count > 0)
        {
            var players = await _context.Players
                .Where(p => request.CustomPlayerIds.Contains(p.Id))
                .ToListAsync();

            return players
                .OrderBy(p => p.FullName)
                .Select(p => new SquadPlayerDto(p.Id, p.FullName))
                .ToList();
        }

        Event? evt = null;
        if (request.EventId.HasValue)
        {
            evt = await _context.Events
                .Include(e => e.Responses)
                    .ThenInclude(r => r.Player)
                .FirstOrDefaultAsync(e => e.Id == request.EventId.Value);
        }

        if (evt == null && request.MatchId.HasValue)
        {
            evt = await _context.Events
                .Include(e => e.Responses)
                    .ThenInclude(r => r.Player)
                .FirstOrDefaultAsync(e => e.MatchId == request.MatchId.Value);

            if (evt == null)
            {
                var match = await _context.Matches.Include(m => m.Event).FirstOrDefaultAsync(m => m.Id == request.MatchId.Value);
                if (match?.EventId != null)
                {
                    evt = await _context.Events
                        .Include(e => e.Responses)
                            .ThenInclude(r => r.Player)
                        .FirstOrDefaultAsync(e => e.Id == match.EventId.Value);
                }
            }
        }

        if (evt != null)
        {
            var attending = evt.Responses
                .Where(r => r.Status == "Attending" && r.Player != null)
                .Select(r => new SquadPlayerDto(r.Player.Id, r.Player.FullName))
                .OrderBy(p => p.Name)
                .ToList();

            if (attending.Count > 0)
            {
                return attending;
            }
        }

        // Fallback: If match has team or appearances, get active team players
        if (request.MatchId.HasValue)
        {
            var match = await _context.Matches.FirstOrDefaultAsync(m => m.Id == request.MatchId.Value);
            if (match?.TeamId != null)
            {
                var teamPlayers = await _context.Players
                    .Where(p => p.TeamId == match.TeamId && p.IsActive)
                    .ToListAsync();

                return teamPlayers
                    .OrderBy(p => p.FullName)
                    .Select(p => new SquadPlayerDto(p.Id, p.FullName))
                    .ToList();
            }
        }

        // Last fallback: all active players
        var allPlayers = await _context.Players
            .Where(p => p.IsActive)
            .ToListAsync();

        return allPlayers
            .OrderBy(p => p.FullName)
            .Select(p => new SquadPlayerDto(p.Id, p.FullName))
            .ToList();
    }

    public record PeriodInterval(int PeriodNumber, int Half, int StartMinute, int EndMinute, int DurationMinutes);

    public static List<PeriodInterval> BuildPeriodIntervals(int halfDurationMinutes)
    {
        var intervals = new List<PeriodInterval>();
        
        List<int> half1Durations;
        if (halfDurationMinutes == 25)
        {
            // 4 periods per half: 6m, 6m, 6m, 7m (total 25m)
            half1Durations = new List<int> { 6, 6, 6, 7 };
        }
        else if (halfDurationMinutes == 20)
        {
            // 3 periods per half: 6m, 6m, 8m (total 20m)
            half1Durations = new List<int> { 6, 6, 8 };
        }
        else if (halfDurationMinutes == 18)
        {
            // 3 periods per half: 6m, 6m, 6m (total 18m)
            half1Durations = new List<int> { 6, 6, 6 };
        }
        else if (halfDurationMinutes == 15)
        {
            // 3 periods per half: 5m, 5m, 5m (total 15m)
            half1Durations = new List<int> { 5, 5, 5 };
        }
        else
        {
            // Generic: 6 min slots with remainder attached to the final slot of the half
            var numPeriods = Math.Max(1, halfDurationMinutes / 6);
            half1Durations = new List<int>();
            var remaining = halfDurationMinutes;
            for (int i = 0; i < numPeriods - 1; i++)
            {
                half1Durations.Add(6);
                remaining -= 6;
            }
            half1Durations.Add(remaining);
        }

        int periodNum = 1;
        int currentMin = 0;

        // Half 1
        foreach (var dur in half1Durations)
        {
            intervals.Add(new PeriodInterval(periodNum++, 1, currentMin, currentMin + dur, dur));
            currentMin += dur;
        }

        // Half 2 (same structure)
        foreach (var dur in half1Durations)
        {
            intervals.Add(new PeriodInterval(periodNum++, 2, currentMin, currentMin + dur, dur));
            currentMin += dur;
        }

        return intervals;
    }

    private static List<SquadPeriodDto> GeneratePeriods(
        List<SquadPlayerDto> players,
        SquadPlayerDto? gk1,
        SquadPlayerDto? gk2,
        int halfDurationMinutes,
        string format = "5v5",
        int? customTotalPeriods = null,
        int? customPeriodMinutes = null)
    {
        var is3v3 = string.Equals(format, "3v3", StringComparison.OrdinalIgnoreCase);

        List<PeriodInterval> intervals;
        if (customTotalPeriods.HasValue && customTotalPeriods.Value > 0 && customPeriodMinutes.HasValue && customPeriodMinutes.Value > 0)
        {
            intervals = new List<PeriodInterval>();
            var pCount = customTotalPeriods.Value;
            var pDur = customPeriodMinutes.Value;
            var customHalfBoundary = pCount / 2;
            for (int i = 1; i <= pCount; i++)
            {
                var half = i <= customHalfBoundary ? 1 : 2;
                var start = (i - 1) * pDur;
                var end = i * pDur;
                intervals.Add(new PeriodInterval(i, half, start, end, pDur));
            }
        }
        else
        {
            intervals = BuildPeriodIntervals(halfDurationMinutes);
        }

        var totalPeriods = intervals.Count;
        var halfBoundary = totalPeriods / 2;
        var targetPitchSize = is3v3 ? Math.Min(3, players.Count) : Math.Min(5, players.Count);
        var targetOutfieldCount = is3v3 ? targetPitchSize : Math.Max(0, targetPitchSize - 1); // 3v3: 3 outfield 0 GK; 5v5: 4 outfield + 1 GK

        var totalMinutesPlayed = players.ToDictionary(p => p.Id, _ => 0);
        var consecutiveMinutes = players.ToDictionary(p => p.Id, _ => 0);

        List<SquadPlayerDto> previousOnPitch = new();
        var periods = new List<SquadPeriodDto>();

        for (int periodIdx = 0; periodIdx < intervals.Count; periodIdx++)
        {
            var interval = intervals[periodIdx];
            var periodIndex = interval.PeriodNumber;
            var isFirstHalf = interval.Half == 1;
            var currentHalf = interval.Half;
            var currentGk = is3v3 ? null : (isFirstHalf ? gk1 : gk2);
            var periodDuration = interval.DurationMinutes;

            var pool = is3v3 
                ? players.ToList() 
                : (currentGk != null ? players.Where(p => p.Id != currentGk.Id).ToList() : players.ToList());

            List<SquadPlayerDto> currentOutfield;
            List<SquadPlayerDto> currentBench;

            if (periodIndex == 1)
            {
                // In Period 1: Starting lineup
                var startingPool = is3v3
                    ? pool.OrderBy(p => p.Name).ToList()
                    : pool.OrderBy(p => (gk2 != null && p.Id == gk2.Id) ? 1 : 0).ThenBy(p => p.Name).ToList();

                currentOutfield = startingPool.Take(targetOutfieldCount).ToList();
                currentBench = startingPool.Skip(targetOutfieldCount).ToList();
            }
            else
            {
                var prevPeriod = periods[periodIdx - 1];
                var prevBench = prevPeriod.BenchPlayers.Where(b => currentGk == null || b.Id != currentGk.Id).ToList();
                var prevOutfield = prevPeriod.OutfieldPlayers.Where(o => currentGk == null || o.Id != currentGk.Id).ToList();

                // Bench size for this period
                var targetBenchCount = Math.Max(0, pool.Count - targetOutfieldCount);
                var targetSubsCount = Math.Min(targetBenchCount, targetOutfieldCount);

                // 1. Bench players from previous period COME ON (up to targetOutfieldCount)
                var candidatePoolToComeOn = new List<SquadPlayerDto>(prevBench);
                var totalMatchMinutes = intervals.Sum(i => i.DurationMinutes);
                var targetAvgMinutes = (totalMatchMinutes * targetPitchSize) / (double)players.Count;
                var targetGkOutfieldMinutes = Math.Max(0, targetAvgMinutes - halfDurationMinutes);

                if (!is3v3 && gk1 != null && currentGk != null && periodIndex == halfBoundary + 1 && gk1.Id != currentGk.Id && targetGkOutfieldMinutes >= periodDuration)
                {
                    candidatePoolToComeOn.Add(gk1);
                }

                int GetEffectiveMinutes(Guid playerId)
                {
                    var min = totalMinutesPlayed[playerId];
                    if (is3v3)
                    {
                        return min;
                    }

                    // If GK in the other half, add penalty once they reached their target outfield minutes
                    if (gk2 != null && isFirstHalf && playerId == gk2.Id)
                    {
                        if (min >= targetGkOutfieldMinutes)
                        {
                            min += halfDurationMinutes;
                        }
                    }
                    else if (gk1 != null && !isFirstHalf && playerId == gk1.Id)
                    {
                        var outfieldMin = min - halfDurationMinutes;
                        if (outfieldMin >= targetGkOutfieldMinutes)
                        {
                            min += 100;
                        }
                        else if (periodIndex == halfBoundary + 1)
                        {
                            // At halftime, let existing bench come on first
                            min += 50;
                        }
                    }
                    return min;
                }

                // Rank incoming candidates by lowest effective minutes
                var incomingPlayers = candidatePoolToComeOn
                    .OrderBy(p => GetEffectiveMinutes(p.Id))
                    .ThenBy(p => consecutiveMinutes[p.Id])
                    .Take(targetSubsCount)
                    .ToList();

                var selectedOutfield = new List<SquadPlayerDto>(incomingPlayers);
                var selectedIds = selectedOutfield.Select(p => p.Id).ToHashSet();

                // 2. Outfield players from previous period who STAY on the pitch
                var remainingNeeded = targetOutfieldCount - selectedOutfield.Count;

                var candidatePoolToStay = prevOutfield
                    .Where(p => !selectedIds.Contains(p.Id))
                    .OrderBy(p => GetEffectiveMinutes(p.Id))
                    .ThenBy(p => consecutiveMinutes[p.Id])
                    .Take(remainingNeeded)
                    .ToList();

                selectedOutfield.AddRange(candidatePoolToStay);
                foreach (var s in candidatePoolToStay) selectedIds.Add(s.Id);

                // 3. Fallback filler if pool changed (e.g. GK switch)
                if (selectedOutfield.Count < targetOutfieldCount)
                {
                    var filler = pool
                        .Where(p => !selectedIds.Contains(p.Id))
                        .OrderBy(p => totalMinutesPlayed[p.Id])
                        .ThenBy(p => consecutiveMinutes[p.Id])
                        .Take(targetOutfieldCount - selectedOutfield.Count)
                        .ToList();

                    selectedOutfield.AddRange(filler);
                    foreach (var f in filler) selectedIds.Add(f.Id);
                }

                currentOutfield = pool.Where(p => selectedIds.Contains(p.Id)).ToList();
                currentBench = pool.Where(p => !selectedIds.Contains(p.Id)).ToList();
            }

            var currentOnPitch = new List<SquadPlayerDto>();
            if (currentGk != null)
            {
                currentOnPitch.Add(currentGk);
            }
            currentOnPitch.AddRange(currentOutfield);

            // Compute substitutions between previous pitch and current pitch
            var prevPitchIds = previousOnPitch.Select(p => p.Id).ToHashSet();
            var currPitchIds = currentOnPitch.Select(p => p.Id).ToHashSet();

            var substitutions = new List<SubstitutionInfoDto>();
            if (periodIndex > 1)
            {
                var playersIn = currentOnPitch.Where(p => !prevPitchIds.Contains(p.Id)).ToList();
                var playersOut = previousOnPitch.Where(p => !currPitchIds.Contains(p.Id)).ToList();

                var count = Math.Min(playersIn.Count, playersOut.Count);
                for (int s = 0; s < count; s++)
                {
                    substitutions.Add(new SubstitutionInfoDto(
                        PlayerInId: playersIn[s].Id,
                        PlayerInName: playersIn[s].Name,
                        PlayerOutId: playersOut[s].Id,
                        PlayerOutName: playersOut[s].Name
                    ));
                }
            }

            // Update stats with exact period duration
            var onPitchSet = currPitchIds;
            foreach (var p in players)
            {
                if (onPitchSet.Contains(p.Id))
                {
                    totalMinutesPlayed[p.Id] += periodDuration;
                    consecutiveMinutes[p.Id] += periodDuration;
                }
                else
                {
                    consecutiveMinutes[p.Id] = 0;
                }
            }

            periods.Add(new SquadPeriodDto(
                PeriodNumber: periodIndex,
                Half: currentHalf,
                StartMinute: interval.StartMinute,
                EndMinute: interval.EndMinute,
                Goalkeeper: currentGk,
                OutfieldPlayers: currentOutfield,
                BenchPlayers: currentBench,
                Substitutions: substitutions
            ));

            previousOnPitch = currentOnPitch;
        }

        return periods;
    }

    private static List<PlayerMinutesDto> CalculateMinutes(
        List<SquadPlayerDto> players,
        List<SquadPeriodDto> periods,
        int defaultPeriodMinutes)
    {
        var result = new List<PlayerMinutesDto>();

        foreach (var player in players)
        {
            int gkMin = 0;
            int outfieldMin = 0;
            int benchMin = 0;

            foreach (var period in periods)
            {
                var dur = period.EndMinute > period.StartMinute 
                    ? (period.EndMinute - period.StartMinute) 
                    : defaultPeriodMinutes;

                if (period.Goalkeeper?.Id == player.Id)
                {
                    gkMin += dur;
                }
                else if (period.OutfieldPlayers.Any(o => o.Id == player.Id))
                {
                    outfieldMin += dur;
                }
                else
                {
                    benchMin += dur;
                }
            }

            result.Add(new PlayerMinutesDto(
                PlayerId: player.Id,
                PlayerName: player.Name,
                TotalMinutes: gkMin + outfieldMin,
                GoalkeeperMinutes: gkMin,
                OutfieldMinutes: outfieldMin,
                BenchMinutes: benchMin
            ));
        }

        return result.OrderByDescending(m => m.TotalMinutes).ThenBy(m => m.PlayerName).ToList();
    }

    private static MatchSquadDto MapToDto(MatchSquad squad)
    {
        var halfDuration = squad.HalfDurationMinutes > 0 ? squad.HalfDurationMinutes : (squad.TotalPeriods * squad.PeriodDurationMinutes) / 2;
        var format = !string.IsNullOrEmpty(squad.Format) ? squad.Format : "5v5";

        try
        {
            var payload = JsonSerializer.Deserialize<SavedSquadPayload>(squad.SquadDataJson, JsonOptions);
            if (payload != null)
            {
                return new MatchSquadDto(
                    Id: squad.Id,
                    MatchId: squad.MatchId,
                    EventId: squad.EventId,
                    HalfDurationMinutes: halfDuration,
                    Format: format,
                    TotalPeriods: squad.TotalPeriods,
                    PeriodDurationMinutes: squad.PeriodDurationMinutes,
                    FirstHalfGoalkeeperPlayerId: squad.FirstHalfGoalkeeperPlayerId,
                    FirstHalfGoalkeeperName: squad.FirstHalfGoalkeeperPlayer?.FullName,
                    SecondHalfGoalkeeperPlayerId: squad.SecondHalfGoalkeeperPlayerId,
                    SecondHalfGoalkeeperName: squad.SecondHalfGoalkeeperPlayer?.FullName,
                    RegisteredPlayers: payload.RegisteredPlayers ?? new List<SquadPlayerDto>(),
                    Periods: payload.Periods ?? new List<SquadPeriodDto>(),
                    PlayerMinutes: payload.PlayerMinutes ?? new List<PlayerMinutesDto>(),
                    UpdatedAt: squad.UpdatedAt
                );
            }
        }
        catch
        {
            // fallback if deserialization fails
        }

        return new MatchSquadDto(
            Id: squad.Id,
            MatchId: squad.MatchId,
            EventId: squad.EventId,
            HalfDurationMinutes: halfDuration,
            Format: format,
            TotalPeriods: squad.TotalPeriods,
            PeriodDurationMinutes: squad.PeriodDurationMinutes,
            FirstHalfGoalkeeperPlayerId: squad.FirstHalfGoalkeeperPlayerId,
            FirstHalfGoalkeeperName: squad.FirstHalfGoalkeeperPlayer?.FullName,
            SecondHalfGoalkeeperPlayerId: squad.SecondHalfGoalkeeperPlayerId,
            SecondHalfGoalkeeperName: squad.SecondHalfGoalkeeperPlayer?.FullName,
            RegisteredPlayers: new List<SquadPlayerDto>(),
            Periods: new List<SquadPeriodDto>(),
            PlayerMinutes: new List<PlayerMinutesDto>(),
            UpdatedAt: squad.UpdatedAt
        );
    }

    private record SavedSquadPayload(
        List<SquadPlayerDto> RegisteredPlayers,
        List<SquadPeriodDto> Periods,
        List<PlayerMinutesDto> PlayerMinutes
    );
}
