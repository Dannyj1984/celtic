using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Celtic.Api.DTOs;

namespace Celtic.IntegrationTests.Players;

public class PlayerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PlayerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<string> GetAdminTokenAsync(HttpClient client)
    {
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("admin@celtic.app", "Admin123!"));
        var result = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return result!.Token;
    }

    private async Task<string> GetParentTokenAsync(HttpClient client)
    {
        // Must authorize as admin first to create the parent
        var adminToken = await GetAdminTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var createParent = new CreateAccountRequest(
            Email: "playerparent@test.com",
            FullName: "Player Parent",
            Password: "Parent123!",
            Phone: null
        );
        var res = await client.PostAsJsonAsync("/api/auth/create-account", createParent);

        client.DefaultRequestHeaders.Authorization = null;
        var parentLogin = new LoginRequest("playerparent@test.com", "Parent123!");
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", parentLogin);
        var result = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return result!.Token;
    }

    [Fact]
    public async Task GetPlayers_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/players");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlayer_AsParent_Returns403()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var parentToken = await GetParentTokenAsync(client);
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentToken);
        var request = new CreatePlayerRequest("Test", "Player", null, null, null, null, null, null);
        var response = await client.PostAsJsonAsync("/api/players", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateAndGetPlayer_AsAdmin_ReturnsSuccess()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var adminToken = await GetAdminTokenAsync(client);
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        
        // Create
        var request = new CreatePlayerRequest("John", "Doe", new DateTime(2018, 5, 12, 0, 0, 0, DateTimeKind.Utc), "No nuts", "Jane Doe", "07700900000", null, null);
        var response = await client.PostAsJsonAsync("/api/players", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var created = await response.Content.ReadFromJsonAsync<PlayerDto>();
        Assert.NotNull(created);
        Assert.Equal("John", created!.FirstName);
        Assert.Equal("Doe", created.LastName);
        Assert.True(created.IsActive);

        // Get by ID
        var getResponse = await client.GetAsync($"/api/players/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var fetched = await getResponse.Content.ReadFromJsonAsync<PlayerDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);

        // Get all
        var getAllResponse = await client.GetAsync("/api/players");
        var list = await getAllResponse.Content.ReadFromJsonAsync<List<PlayerDto>>();
        Assert.NotNull(list);
        Assert.Contains(list!, p => p.Id == created.Id);
    }

    [Fact]
    public async Task UpdatePlayer_AsAdmin_ReturnsSuccess()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var adminToken = await GetAdminTokenAsync(client);
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        
        // Create
        var createRequest = new CreatePlayerRequest("John", "Doe", null, null, null, null, null, null);
        var response = await client.PostAsJsonAsync("/api/players", createRequest);
        var created = await response.Content.ReadFromJsonAsync<PlayerDto>();

        // Update
        var updateRequest = new UpdatePlayerRequest("John", "Smith", null, "Updated notes", null, null, null, null, false, "Active");
        var updateResponse = await client.PutAsJsonAsync($"/api/players/{created!.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<PlayerDto>();
        Assert.NotNull(updated);
        Assert.Equal("Smith", updated!.LastName);
        Assert.False(updated.IsActive);
        Assert.Equal("Updated notes", updated.MedicalNotes);
    }
}
