using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Celtic.Api.DTOs;

namespace Celtic.IntegrationTests.Auth;

public class MeTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MeTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var loginRequest = new LoginRequest("admin@celtic.app", "Admin123!");
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result!.Token;
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsUserInfo()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UserInfoResponse>();
        Assert.NotNull(result);
        Assert.Equal("admin@celtic.app", result!.Email);
        Assert.Equal("Team Admin", result.FullName);
        Assert.Equal("Admin", result.Role);
        Assert.NotNull(result.Children);
        Assert.Empty(result.Children);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
