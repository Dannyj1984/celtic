using System;
using System.Linq;
using System.Threading.Tasks;
using Celtic.Api.Data;
using Celtic.Api.DTOs;
using Celtic.Api.Models;
using Celtic.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Celtic.UnitTests.Services;

public class TeamServiceTests
{
    private CelticDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CelticDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CelticDbContext(options);
    }

    [Fact]
    public async Task CreateTeam_And_GetAllTeams_WorkCorrectly()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);
        var service = new TeamService(dbContext);

        // Act
        var created1 = await service.CreateTeamAsync(new CreateTeamRequest { Name = "Stripes", ColorHex = "#006837" });
        var created2 = await service.CreateTeamAsync(new CreateTeamRequest { Name = "Hoops", ColorHex = "#F59E0B" });

        var all = await service.GetAllTeamsAsync();

        // Assert
        Assert.Equal(2, all.Count);
        Assert.Contains(all, t => t.Name == "Stripes");
        Assert.Contains(all, t => t.Name == "Hoops");
    }

    [Fact]
    public async Task DeleteTeam_UnlinksPlayersAndMatches()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);

        var team = new Team { Id = Guid.NewGuid(), Name = "Stripes" };
        dbContext.Teams.Add(team);

        var player = new Player { Id = Guid.NewGuid(), FirstName = "Joe", LastName = "Bloggs", TeamId = team.Id };
        dbContext.Players.Add(player);

        await dbContext.SaveChangesAsync();

        var service = new TeamService(dbContext);

        // Act
        await service.DeleteTeamAsync(team.Id);

        // Assert
        var updatedPlayer = await dbContext.Players.FindAsync(player.Id);
        Assert.NotNull(updatedPlayer);
        Assert.Null(updatedPlayer.TeamId);

        var remainingTeams = await service.GetAllTeamsAsync();
        Assert.Empty(remainingTeams);
    }

    [Fact]
    public async Task GetAllTeams_CountsMultiTeamPlayersCorrectly()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);

        var team1 = new Team { Id = Guid.NewGuid(), Name = "Stripes" };
        var team2 = new Team { Id = Guid.NewGuid(), Name = "Hoops" };
        dbContext.Teams.AddRange(team1, team2);

        var player1 = new Player { Id = Guid.NewGuid(), FirstName = "Joe", LastName = "One", IsActive = true };
        var player2 = new Player { Id = Guid.NewGuid(), FirstName = "Sam", LastName = "Two", IsActive = true };
        dbContext.Players.AddRange(player1, player2);

        // player1 plays for both team1 and team2, player2 only team1
        dbContext.PlayerTeams.AddRange(
            new PlayerTeam { PlayerId = player1.Id, TeamId = team1.Id },
            new PlayerTeam { PlayerId = player1.Id, TeamId = team2.Id },
            new PlayerTeam { PlayerId = player2.Id, TeamId = team1.Id }
        );
        await dbContext.SaveChangesAsync();

        var service = new TeamService(dbContext);

        // Act
        var teams = await service.GetAllTeamsAsync();

        // Assert
        var stripes = teams.First(t => t.Name == "Stripes");
        var hoops = teams.First(t => t.Name == "Hoops");
        Assert.Equal(2, stripes.PlayersCount);
        Assert.Equal(1, hoops.PlayersCount);

        // Delete team2, verify player1's association with team2 is removed, but team1 remains
        await service.DeleteTeamAsync(team2.Id);
        var remaining = await service.GetAllTeamsAsync();
        Assert.Single(remaining);
        Assert.Equal("Stripes", remaining[0].Name);
        Assert.Equal(2, remaining[0].PlayersCount);
    }
}
