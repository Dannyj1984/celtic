using Microsoft.EntityFrameworkCore;
using Celtic.Api.Data;
using Celtic.Api.DTOs;
using Celtic.Api.Models;

namespace Celtic.Api.Services;

public class SeasonService : ISeasonService
{
    private readonly CelticDbContext _db;

    public SeasonService(CelticDbContext db)
    {
        _db = db;
    }

    public async Task<List<SeasonDto>> GetAllSeasonsAsync()
    {
        var seasons = await _db.Seasons
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();

        return seasons.Select(MapToDto).ToList();
    }

    public async Task<SeasonDto> GetSeasonByIdAsync(Guid id)
    {
        var season = await _db.Seasons.FindAsync(id);
        if (season == null)
            throw new KeyNotFoundException("Season not found");

        return MapToDto(season);
    }

    public async Task<SeasonDto> CreateSeasonAsync(CreateSeasonRequest request)
    {
        // If this season is current, we might want to un-set others
        if (request.IsCurrent)
        {
            await UnsetCurrentSeasonsAsync();
        }

        var season = new Season
        {
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            SubAmount = request.SubAmount,
            SubFrequency = request.SubFrequency,
            IsCurrent = request.IsCurrent
        };

        _db.Seasons.Add(season);
        await _db.SaveChangesAsync();

        return MapToDto(season);
    }

    public async Task<SeasonDto> UpdateSeasonAsync(Guid id, UpdateSeasonRequest request)
    {
        var season = await _db.Seasons.FindAsync(id);
        if (season == null)
            throw new KeyNotFoundException("Season not found");

        if (request.IsCurrent && !season.IsCurrent)
        {
            await UnsetCurrentSeasonsAsync();
        }

        season.Name = request.Name;
        season.StartDate = request.StartDate;
        season.EndDate = request.EndDate;
        season.SubAmount = request.SubAmount;
        season.SubFrequency = request.SubFrequency;
        season.IsCurrent = request.IsCurrent;

        await _db.SaveChangesAsync();

        return MapToDto(season);
    }

    private async Task UnsetCurrentSeasonsAsync()
    {
        var currentSeasons = await _db.Seasons.Where(s => s.IsCurrent).ToListAsync();
        foreach (var s in currentSeasons)
        {
            s.IsCurrent = false;
        }
    }

    private static SeasonDto MapToDto(Season s) => new(
        s.Id,
        s.Name,
        s.StartDate,
        s.EndDate,
        s.SubAmount,
        s.SubFrequency,
        s.IsCurrent
    );
}
