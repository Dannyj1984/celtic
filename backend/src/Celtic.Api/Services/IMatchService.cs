using Celtic.Api.DTOs;

namespace Celtic.Api.Services;

public interface IMatchService
{
    Task<List<MatchDto>> GetAllMatchesAsync();
    Task<MatchDto> GetMatchByIdAsync(Guid id);
    Task<MatchDto> CreateMatchAsync(CreateMatchRequest request);
    Task<MatchDto> UpdateMatchAsync(Guid id, UpdateMatchRequest request);
    Task DeleteMatchAsync(Guid id);
}
