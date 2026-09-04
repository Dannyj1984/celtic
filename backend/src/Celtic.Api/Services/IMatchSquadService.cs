using Celtic.Api.DTOs;

namespace Celtic.Api.Services;

public interface IMatchSquadService
{
    Task<MatchSquadDto> GenerateSquadAsync(GenerateMatchSquadRequest request);
    Task<MatchSquadDto?> GetSquadByMatchIdAsync(Guid matchId);
    Task<MatchSquadDto?> GetSquadByEventIdAsync(Guid eventId);
    Task<MatchSquadDto> SaveSquadAsync(Guid? matchId, Guid? eventId, SaveMatchSquadRequest request);
}
