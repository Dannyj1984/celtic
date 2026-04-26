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
            .ToListAsync();

        return players.Select(MapToDto).ToList();
    }

    public async Task<PlayerDto> GetPlayerByIdAsync(Guid id)
    {
        var player = await _db.Players.FindAsync(id);
        if (player == null)
            throw new KeyNotFoundException("Player not found");

        return MapToDto(player);
    }

    public async Task<PlayerDto> CreatePlayerAsync(CreatePlayerRequest request)
    {
        var player = new Player
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            MedicalNotes = request.MedicalNotes,
            EmergencyContact = request.EmergencyContact,
            EmergencyPhone = request.EmergencyPhone,
            IsActive = true
        };

        _db.Players.Add(player);
        await _db.SaveChangesAsync();

        return MapToDto(player);
    }

    public async Task<PlayerDto> UpdatePlayerAsync(Guid id, UpdatePlayerRequest request)
    {
        var player = await _db.Players.FindAsync(id);
        if (player == null)
            throw new KeyNotFoundException("Player not found");

        player.FirstName = request.FirstName;
        player.LastName = request.LastName;
        player.DateOfBirth = request.DateOfBirth;
        player.MedicalNotes = request.MedicalNotes;
        player.EmergencyContact = request.EmergencyContact;
        player.EmergencyPhone = request.EmergencyPhone;
        player.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        return MapToDto(player);
    }

    private static PlayerDto MapToDto(Player p) => new(
        p.Id,
        p.FirstName,
        p.LastName,
        p.DateOfBirth,
        p.MedicalNotes,
        p.EmergencyContact,
        p.EmergencyPhone,
        p.IsActive
    );
}
