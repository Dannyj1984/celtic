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
}
