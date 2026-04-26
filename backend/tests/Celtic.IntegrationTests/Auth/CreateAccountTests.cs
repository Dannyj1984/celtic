using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Celtic.Api.DTOs;

namespace Celtic.IntegrationTests.Auth;

public class CreateAccountTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CreateAccountTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<string> GetAdminTokenAsync(HttpClient client)
    {
        var loginRequest = new LoginRequest("admin@celtic.app", "Admin123!");
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result!.Token;
    }

    [Fact]
    public async Task CreateAccount_AsAdmin_Returns201()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateAccountRequest(
            Email: "parent@test.com",
            FullName: "John Smith",
            Password: "Parent123!",
            Phone: "07123456789"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/create-account", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CreateAccountResponse>();
        Assert.NotNull(result);
        Assert.Equal("parent@test.com", result!.Email);
        Assert.Equal("John Smith", result.FullName);
        Assert.Equal("Parent", result.Role);
        Assert.NotEmpty(result.UserId);
    }

    [Fact]
    public async Task CreateAccount_WithoutAuth_Returns401()
    {
        // Arrange — no auth token
        var client = _factory.CreateClient();
        var request = new CreateAccountRequest(
            Email: "nobody@test.com",
            FullName: "Nobody",
            Password: "Test123!",
            Phone: null
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/create-account", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_AsParent_Returns403()
    {
        // Use own factory to avoid shared state
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Arrange — create a parent account first, then try to create another
        var adminToken = await GetAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Create parent
        var createParent = new CreateAccountRequest(
            Email: "parent2@test.com",
            FullName: "Parent User",
            Password: "Parent123!",
            Phone: null
        );
        await client.PostAsJsonAsync("/api/auth/create-account", createParent);

        // Login as parent
        client.DefaultRequestHeaders.Authorization = null;
        var parentLogin = new LoginRequest("parent2@test.com", "Parent123!");
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", parentLogin);
        var parentResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Try to create another account as parent
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentResult!.Token);
        var request = new CreateAccountRequest(
            Email: "sneaky@test.com",
            FullName: "Sneaky User",
            Password: "Sneaky123!",
            Phone: null
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/create-account", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_DuplicateEmail_Returns400()
    {
        // Use own factory to avoid shared state
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Arrange
        var token = await GetAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateAccountRequest(
            Email: "duplicate@test.com",
            FullName: "First User",
            Password: "Test123!",
            Phone: null
        );

        // Create first account
        await client.PostAsJsonAsync("/api/auth/create-account", request);

        // Act — try to create with same email
        var response = await client.PostAsJsonAsync("/api/auth/create-account", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
