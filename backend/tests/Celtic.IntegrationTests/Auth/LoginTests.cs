using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Celtic.Api.DTOs;

namespace Celtic.IntegrationTests.Auth;

public class LoginTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LoginTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndUserInfo()
    {
        // Arrange — first create an admin to login with
        // The app seeds an admin on startup: admin@celtic.app / Admin123!
        var loginRequest = new LoginRequest("admin@celtic.app", "Admin123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Token);
        Assert.Equal("admin@celtic.app", result.Email);
        Assert.Equal("Admin", result.Role);
        Assert.Equal("Team Admin", result.FullName);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401()
    {
        // Arrange
        var loginRequest = new LoginRequest("admin@celtic.app", "WrongPassword");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_Returns401()
    {
        // Arrange
        var loginRequest = new LoginRequest("nobody@celtic.app", "Password123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
