using Celtic.Api.DTOs;

namespace Celtic.Api.Services;

public interface IPlayerService
{
    Task<List<PlayerDto>> GetAllPlayersAsync();
    Task<PlayerDto> GetPlayerByIdAsync(Guid id);
    Task<PlayerDto> CreatePlayerAsync(CreatePlayerRequest request);
    Task<PlayerDto> UpdatePlayerAsync(Guid id, UpdatePlayerRequest request);
    Task<PlayerDto> UpdateSubscriptionStatusAsync(Guid id, string status);
}
