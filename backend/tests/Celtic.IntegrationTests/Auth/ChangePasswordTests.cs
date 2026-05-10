using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Celtic.Api.DTOs;

namespace Celtic.IntegrationTests.Auth;

public class ChangePasswordTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ChangePasswordTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ChangePassword_WithValidCurrentPassword_Returns200()
    {
        // Use a separate factory to avoid shared state issues
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Arrange — login as admin
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("admin@celtic.app", "Admin123!"));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);

        var request = new ChangePasswordRequest("Admin123!", "NewAdmin456!");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/change-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify can login with new password
        client.DefaultRequestHeaders.Authorization = null;
        var newLoginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("admin@celtic.app", "NewAdmin456!"));
        Assert.Equal(HttpStatusCode.OK, newLoginResponse.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_Returns400()
    {
        // Use a separate factory to avoid shared state issues
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Arrange — login as admin
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("admin@celtic.app", "Admin123!"));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);

        var request = new ChangePasswordRequest("WrongPassword", "NewPassword123!");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/change-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        // Arrange
        var request = new ChangePasswordRequest("Whatever", "NewPassword123!");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/change-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithTooShortNewPassword_Returns400()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Arrange — login as admin
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("admin@celtic.app", "Admin123!"));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);

        var request = new ChangePasswordRequest("Admin123!", "123"); // Too short

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/change-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
