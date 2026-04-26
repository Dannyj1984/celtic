using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Celtic.Api.DTOs;

namespace Celtic.IntegrationTests.Seasons;

public class SeasonTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SeasonTests(CustomWebApplicationFactory factory)
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
            Email: "seasonparent@test.com",
            FullName: "Season Parent",
            Password: "Parent123!",
            Phone: null
        );
        var res = await client.PostAsJsonAsync("/api/auth/create-account", createParent);

        client.DefaultRequestHeaders.Authorization = null;
        var parentLogin = new LoginRequest("seasonparent@test.com", "Parent123!");
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", parentLogin);
        var result = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return result!.Token;
    }

    [Fact]
    public async Task GetSeasons_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/seasons");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateSeason_AsParent_Returns403()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var parentToken = await GetParentTokenAsync(client);
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentToken);
        var request = new CreateSeasonRequest("2026-27", DateTime.UtcNow, DateTime.UtcNow.AddYears(1), 50, "Monthly", false);
        var response = await client.PostAsJsonAsync("/api/seasons", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateAndGetSeason_AsAdmin_ReturnsSuccess()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var adminToken = await GetAdminTokenAsync(client);
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        
        // Create
        var request = new CreateSeasonRequest("2026-27", DateTime.UtcNow, DateTime.UtcNow.AddYears(1), 50, "Monthly", true);
        var response = await client.PostAsJsonAsync("/api/seasons", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var created = await response.Content.ReadFromJsonAsync<SeasonDto>();
        Assert.NotNull(created);
        Assert.Equal("2026-27", created!.Name);
        Assert.Equal(50, created.SubAmount);
        Assert.True(created.IsCurrent);

        // Get by ID
        var getResponse = await client.GetAsync($"/api/seasons/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var fetched = await getResponse.Content.ReadFromJsonAsync<SeasonDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);

        // Get all
        var getAllResponse = await client.GetAsync("/api/seasons");
        var list = await getAllResponse.Content.ReadFromJsonAsync<List<SeasonDto>>();
        Assert.NotNull(list);
        Assert.Contains(list!, s => s.Id == created.Id);
    }

    [Fact]
    public async Task UpdateSeason_AsAdmin_ReturnsSuccess()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var adminToken = await GetAdminTokenAsync(client);
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        
        // Create
        var createRequest = new CreateSeasonRequest("Old Season", DateTime.UtcNow, DateTime.UtcNow.AddYears(1), 20, "Weekly", false);
        var response = await client.PostAsJsonAsync("/api/seasons", createRequest);
        var created = await response.Content.ReadFromJsonAsync<SeasonDto>();

        // Update
        var updateRequest = new UpdateSeasonRequest("Updated Season", createRequest.StartDate, createRequest.EndDate, 30, "Weekly", true);
        var updateResponse = await client.PutAsJsonAsync($"/api/seasons/{created!.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<SeasonDto>();
        Assert.NotNull(updated);
        Assert.Equal("Updated Season", updated!.Name);
        Assert.Equal(30, updated.SubAmount);
        Assert.True(updated.IsCurrent);
    }
}
