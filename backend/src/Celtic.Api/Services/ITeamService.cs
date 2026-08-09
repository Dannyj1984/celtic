using Celtic.Api.DTOs;

namespace Celtic.Api.Services;

public interface ITeamService
{
    Task<List<TeamDto>> GetAllTeamsAsync();
    Task<TeamDto> GetTeamByIdAsync(Guid id);
    Task<TeamDto> CreateTeamAsync(CreateTeamRequest request);
    Task<TeamDto> UpdateTeamAsync(Guid id, UpdateTeamRequest request);
    Task DeleteTeamAsync(Guid id);
}
