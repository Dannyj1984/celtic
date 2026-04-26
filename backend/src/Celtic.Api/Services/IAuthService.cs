using Celtic.Api.DTOs;

namespace Celtic.Api.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<CreateAccountResponse> CreateAccountAsync(CreateAccountRequest request);
    Task<UserInfoResponse> GetUserInfoAsync(string userId);
    Task<List<UserInfoResponse>> GetAllParentsAsync();
    Task ChangePasswordAsync(string userId, ChangePasswordRequest request);
}
