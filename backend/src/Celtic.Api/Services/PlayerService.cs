using Microsoft.EntityFrameworkCore;
using Celtic.Api.Data;
using Celtic.Api.DTOs;
using Celtic.Api.Models;

namespace Celtic.Api.Services;

public class PlayerService : IPlayerService
{
    private readonly CelticDbContext _db;

    public PlayerService(CelticDbContext db)
    {
        _db = db;
    }

    public async Task<List<PlayerDto>> GetAllPlayersAsync()
    {
        var players = await _db.Players
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Include(p => p.Team)
            .Include(p => p.PlayerTeams)
                .ThenInclude(pt => pt.Team)
            .Include(p => p.ParentLinks)
                .ThenInclude(pl => pl.User)
            .Include(p => p.EventResponses)
            .ToListAsync();

        var trainingIds = await GetRecentEventIds("Training", 10);
        var matchIds = await GetRecentEventIds("Match", 10);

        return players.Select(p => MapToDto(p, trainingIds, matchIds)).ToList();
    }

    public async Task<PlayerDto> GetPlayerByIdAsync(Guid id)
    {
        var player = await _db.Players
            .Include(p => p.Team)
            .Include(p => p.PlayerTeams)
                .ThenInclude(pt => pt.Team)
            .Include(p => p.ParentLinks)
                .ThenInclude(pl => pl.User)
            .Include(p => p.EventResponses)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (player == null)
            throw new KeyNotFoundException("Player not found");

        var trainingIds = await GetRecentEventIds("Training", 10);
        var matchIds = await GetRecentEventIds("Match", 10);

        return MapToDto(player, trainingIds, matchIds);
    }

    private async Task<List<Guid>> GetRecentEventIds(string type, int count)
    {
        return await _db.Events
            .Where(e => e.Type == type && e.DateTime <= DateTime.UtcNow && !e.IsCancelled)
            .OrderByDescending(e => e.DateTime)
            .Take(count)
            .Select(e => e.Id)
            .ToListAsync();
    }

    public async Task<PlayerDto> CreatePlayerAsync(CreatePlayerRequest request)
    {
        var targetTeamIds = (request.TeamIds != null && request.TeamIds.Count > 0)
            ? request.TeamIds.Distinct().ToList()
            : (request.TeamId.HasValue ? new List<Guid> { request.TeamId.Value } : new List<Guid>());

        var player = new Player
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            MedicalNotes = request.MedicalNotes,
            EmergencyContact = request.EmergencyContact,
            EmergencyPhone = request.EmergencyPhone,
            EmergencyContact2 = request.EmergencyContact2,
            EmergencyPhone2 = request.EmergencyPhone2,
            IsActive = true,
            PreferredFoot = request.PreferredFoot,
            CoachNotes = request.CoachNotes,
            FanNumber = request.FanNumber,
            ShirtSize = request.ShirtSize,
            ShortSize = request.ShortSize,
            SockSize = request.SockSize,
            Allergies = request.Allergies,
            AllowPhotos = request.AllowPhotos,
            TrainingCardsCount = request.TrainingCardsCount,
            TeamId = targetTeamIds.Count > 0 ? targetTeamIds[0] : null
        };

        _db.Players.Add(player);

        foreach (var tId in targetTeamIds)
        {
            player.PlayerTeams.Add(new PlayerTeam
            {
                PlayerId = player.Id,
                TeamId = tId
            });
        }

        await _db.SaveChangesAsync();

        if (player.TeamId.HasValue)
        {
            await _db.Entry(player).Reference(p => p.Team).LoadAsync();
        }

        if (player.PlayerTeams.Count > 0)
        {
            await _db.Entry(player).Collection(p => p.PlayerTeams).Query().Include(pt => pt.Team).LoadAsync();
        }

        return MapToDto(player, new List<Guid>(), new List<Guid>());
    }

    public async Task<PlayerDto> UpdatePlayerAsync(Guid id, UpdatePlayerRequest request)
    {
        var player = await _db.Players
            .Include(p => p.Team)
            .Include(p => p.PlayerTeams)
                .ThenInclude(pt => pt.Team)
            .Include(p => p.ParentLinks)
                .ThenInclude(pl => pl.User)
            .Include(p => p.EventResponses)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (player == null)
            throw new KeyNotFoundException("Player not found");

        var targetTeamIds = (request.TeamIds != null)
            ? request.TeamIds.Distinct().ToList()
            : (request.TeamId.HasValue ? new List<Guid> { request.TeamId.Value } : new List<Guid>());

        player.FirstName = request.FirstName;
        player.LastName = request.LastName;
        player.DateOfBirth = request.DateOfBirth;
        player.MedicalNotes = request.MedicalNotes;
        player.EmergencyContact = request.EmergencyContact;
        player.EmergencyPhone = request.EmergencyPhone;
        player.EmergencyContact2 = request.EmergencyContact2;
        player.EmergencyPhone2 = request.EmergencyPhone2;
        player.IsActive = request.IsActive;
        player.SubscriptionStatus = request.SubscriptionStatus;
        player.PreferredFoot = request.PreferredFoot;
        player.CoachNotes = request.CoachNotes;
        player.FanNumber = request.FanNumber;
        player.ShirtSize = request.ShirtSize;
        player.ShortSize = request.ShortSize;
        player.SockSize = request.SockSize;
        player.Allergies = request.Allergies;
        player.AllowPhotos = request.AllowPhotos;
        if (request.TrainingCardsCount.HasValue)
        {
            player.TrainingCardsCount = Math.Max(0, request.TrainingCardsCount.Value);
        }
        player.TeamId = targetTeamIds.Count > 0 ? targetTeamIds[0] : null;

        // Synchronize PlayerTeams
        var existingTeamIds = player.PlayerTeams.Select(pt => pt.TeamId).ToHashSet();
        var toRemove = player.PlayerTeams.Where(pt => !targetTeamIds.Contains(pt.TeamId)).ToList();
        foreach (var item in toRemove)
        {
            _db.PlayerTeams.Remove(item);
        }

        foreach (var tId in targetTeamIds.Where(tid => !existingTeamIds.Contains(tid)))
        {
            player.PlayerTeams.Add(new PlayerTeam
            {
                PlayerId = player.Id,
                TeamId = tId
            });
        }

        await _db.SaveChangesAsync();

        if (player.TeamId.HasValue && (player.Team == null || player.Team.Id != player.TeamId))
        {
            await _db.Entry(player).Reference(p => p.Team).LoadAsync();
        }

        await _db.Entry(player).Collection(p => p.PlayerTeams).Query().Include(pt => pt.Team).LoadAsync();

        var trainingIds = await GetRecentEventIds("Training", 10);
        var matchIds = await GetRecentEventIds("Match", 10);

        return MapToDto(player, trainingIds, matchIds);
    }

    public async Task<PlayerDto> UpdateSubscriptionStatusAsync(Guid id, string status)
    {
        var player = await _db.Players
            .Include(p => p.Team)
            .Include(p => p.PlayerTeams)
                .ThenInclude(pt => pt.Team)
            .Include(p => p.ParentLinks)
                .ThenInclude(pl => pl.User)
            .Include(p => p.EventResponses)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (player == null)
            throw new KeyNotFoundException("Player not found");

        player.SubscriptionStatus = status;
        await _db.SaveChangesAsync();

        var trainingIds = await GetRecentEventIds("Training", 10);
        var matchIds = await GetRecentEventIds("Match", 10);

        return MapToDto(player, trainingIds, matchIds);
    }

    public async Task<PlayerDto> UpdateTrainingCardsAsync(Guid id, int cardsCount)
    {
        var player = await _db.Players
            .Include(p => p.Team)
            .Include(p => p.PlayerTeams)
                .ThenInclude(pt => pt.Team)
            .Include(p => p.ParentLinks)
                .ThenInclude(pl => pl.User)
            .Include(p => p.EventResponses)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (player == null)
            throw new KeyNotFoundException("Player not found");

        player.TrainingCardsCount = Math.Max(0, cardsCount);
        await _db.SaveChangesAsync();

        var trainingIds = await GetRecentEventIds("Training", 10);
        var matchIds = await GetRecentEventIds("Match", 10);

        return MapToDto(player, trainingIds, matchIds);
    }

    private static PlayerDto MapToDto(Player p, List<Guid> recentTrainingIds, List<Guid> recentMatchIds)
    {
        var trainingAttended = p.EventResponses
            .Count(er => er.Status == "Attending" && recentTrainingIds.Contains(er.EventId));
        
        var matchAttended = p.EventResponses
            .Count(er => er.Status == "Attending" && recentMatchIds.Contains(er.EventId));

        var attendance = new PlayerAttendanceDto(
            trainingAttended,
            recentTrainingIds.Count,
            matchAttended,
            recentMatchIds.Count
        );

        var teams = p.PlayerTeams
            .Where(pt => pt.Team != null)
            .Select(pt => new TeamSummaryDto(pt.Team.Id, pt.Team.Name, pt.Team.ColorHex))
            .ToList();

        if (teams.Count == 0 && p.Team != null)
        {
            teams.Add(new TeamSummaryDto(p.Team.Id, p.Team.Name, p.Team.ColorHex));
        }

        var teamIds = teams.Select(t => t.Id).ToList();
        var primaryTeamId = p.TeamId ?? (teamIds.Count > 0 ? teamIds[0] : (Guid?)null);
        var primaryTeamName = p.Team?.Name ?? teams.FirstOrDefault()?.Name;

        return new PlayerDto(
            p.Id,
            p.FirstName,
            p.LastName,
            p.DateOfBirth,
            p.MedicalNotes,
            p.EmergencyContact,
            p.EmergencyPhone,
            p.EmergencyContact2,
            p.EmergencyPhone2,
            p.IsActive,
            p.SubscriptionStatus,
            p.ParentLinks.Select(pl => new PlayerParentDto(
                pl.UserId,
                pl.User.FullName,
                pl.User.Email!,
                pl.User.Phone,
                pl.Relationship
            )).ToList(),
            attendance,
            p.PreferredFoot,
            p.CoachNotes,
            p.FanNumber,
            p.ShirtSize,
            p.ShortSize,
            p.SockSize,
            p.Allergies,
            p.AllowPhotos,
            p.TrainingCardsCount,
            primaryTeamId,
            primaryTeamName,
            teams,
            teamIds
        );
    }
}
