using Celtic.Api.DTOs;

namespace Celtic.Api.Services;

public interface ISeasonService
{
    Task<List<SeasonDto>> GetAllSeasonsAsync();
    Task<SeasonDto> GetSeasonByIdAsync(Guid id);
    Task<SeasonDto> CreateSeasonAsync(CreateSeasonRequest request);
    Task<SeasonDto> UpdateSeasonAsync(Guid id, UpdateSeasonRequest request);
}
