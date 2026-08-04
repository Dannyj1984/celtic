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
        if (!loginResponse.IsSuccessStatusCode)
        {
            var err = await loginResponse.Content.ReadAsStringAsync();
            throw new Exception($"Admin login failed with status {loginResponse.StatusCode}: {err}");
        }
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
        var client = _factory.CreateClient();
        var parentToken = await GetParentTokenAsync(client);
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentToken);
        var request = new CreatePlayerRequest("Test", "Player", null, null, null, null, null, null, "Right", null, null, null, null);
        var response = await client.PostAsJsonAsync("/api/players", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateAndGetPlayer_AsAdmin_ReturnsSuccess()
    {
        var client = _factory.CreateClient();
        var adminToken = await GetAdminTokenAsync(client);
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        
        // Create
        var request = new CreatePlayerRequest("John", "Doe", new DateTime(2018, 5, 12, 0, 0, 0, DateTimeKind.Utc), "Asthma", "Jane Doe", "07700900000", null, null, "Right", null, "FAN123456", "YM", "Peanuts");
        var response = await client.PostAsJsonAsync("/api/players", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var created = await response.Content.ReadFromJsonAsync<PlayerDto>();
        Assert.NotNull(created);
        Assert.Equal("John", created!.FirstName);
        Assert.Equal("Doe", created.LastName);
        Assert.True(created.IsActive);
        Assert.Equal("FAN123456", created.FanNumber);
        Assert.Equal("YM", created.ShirtSize);
        Assert.Equal("Peanuts", created.Allergies);

        // Get by ID
        var getResponse = await client.GetAsync($"/api/players/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var fetched = await getResponse.Content.ReadFromJsonAsync<PlayerDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("FAN123456", fetched.FanNumber);
        Assert.Equal("YM", fetched.ShirtSize);
        Assert.Equal("Peanuts", fetched.Allergies);

        // Get all
        var getAllResponse = await client.GetAsync("/api/players");
        var list = await getAllResponse.Content.ReadFromJsonAsync<List<PlayerDto>>();
        Assert.NotNull(list);
        Assert.Contains(list!, p => p.Id == created.Id);
    }

    [Fact]
    public async Task UpdatePlayer_AsAdmin_ReturnsSuccess()
    {
        var client = _factory.CreateClient();
        var adminToken = await GetAdminTokenAsync(client);
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        
        // Create
        var createRequest = new CreatePlayerRequest("John", "Doe", null, null, null, null, null, null, "Right", null, null, null, null);
        var response = await client.PostAsJsonAsync("/api/players", createRequest);
        var created = await response.Content.ReadFromJsonAsync<PlayerDto>();

        // Update
        var updateRequest = new UpdatePlayerRequest("John", "Smith", null, "Updated notes", null, null, null, null, false, "Active", "Right", null, "FAN999", "YXL", "None");
        var updateResponse = await client.PutAsJsonAsync($"/api/players/{created!.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<PlayerDto>();
        Assert.NotNull(updated);
        Assert.Equal("Smith", updated!.LastName);
        Assert.False(updated.IsActive);
        Assert.Equal("Updated notes", updated.MedicalNotes);
        Assert.Equal("FAN999", updated.FanNumber);
        Assert.Equal("YXL", updated.ShirtSize);
        Assert.Equal("None", updated.Allergies);
    }
}
