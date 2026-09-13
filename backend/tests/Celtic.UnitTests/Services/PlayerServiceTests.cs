using System;
using System.Threading.Tasks;
using Celtic.Api.Data;
using Celtic.Api.DTOs;
using Celtic.Api.Models;
using Celtic.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Celtic.UnitTests.Services;

public class PlayerServiceTests
{
    private CelticDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CelticDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CelticDbContext(options);
    }

    [Fact]
    public async Task CreatePlayerAsync_SavesKitSizing()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);
        var service = new PlayerService(dbContext);

        var request = new CreatePlayerRequest(
            FirstName: "Erling",
            LastName: "Haaland",
            DateOfBirth: new DateTime(2018, 5, 1),
            MedicalNotes: null,
            EmergencyContact: "Alf-Inge",
            EmergencyPhone: "07123456789",
            EmergencyContact2: null,
            EmergencyPhone2: null,
            PreferredFoot: "Left",
            ShirtSize: "7-8 yrs",
            ShortSize: "7-8 yrs",
            SockSize: 12
        );

        // Act
        var result = await service.CreatePlayerAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Erling", result.FirstName);
        Assert.Equal("7-8 yrs", result.ShirtSize);
        Assert.Equal("7-8 yrs", result.ShortSize);
        Assert.Equal(12, result.SockSize);
    }

    [Fact]
    public async Task UpdatePlayerAsync_UpdatesKitSizing()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);
        var player = new Player
        {
            Id = Guid.NewGuid(),
            FirstName = "Kylian",
            LastName = "Mbappe",
            ShirtSize = "5-6 yrs",
            ShortSize = "5-6 yrs",
            SockSize = 10
        };
        dbContext.Players.Add(player);
        await dbContext.SaveChangesAsync();

        var service = new PlayerService(dbContext);
        var request = new UpdatePlayerRequest(
            FirstName: "Kylian",
            LastName: "Mbappe",
            DateOfBirth: null,
            MedicalNotes: null,
            EmergencyContact: null,
            EmergencyPhone: null,
            EmergencyContact2: null,
            EmergencyPhone2: null,
            IsActive: true,
            SubscriptionStatus: "Active",
            PreferredFoot: "Right",
            ShirtSize: "9-10 yrs",
            ShortSize: "9-10 yrs",
            SockSize: 1,
            AllowPhotos: true
        );

        // Act
        var result = await service.UpdatePlayerAsync(player.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("9-10 yrs", result.ShirtSize);
        Assert.Equal("9-10 yrs", result.ShortSize);
        Assert.Equal(1, result.SockSize);
    }

    [Fact]
    public async Task CreatePlayerAsync_WithMultipleTeams_PersistsAllTeams()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);
        var team1 = new Team { Id = Guid.NewGuid(), Name = "Stripes", ColorHex = "#006837" };
        var team2 = new Team { Id = Guid.NewGuid(), Name = "Hoops", ColorHex = "#F59E0B" };
        dbContext.Teams.AddRange(team1, team2);
        await dbContext.SaveChangesAsync();

        var service = new PlayerService(dbContext);
        var request = new CreatePlayerRequest(
            FirstName: "Marcus",
            LastName: "Rashford",
            DateOfBirth: new DateTime(2018, 1, 1),
            MedicalNotes: null,
            EmergencyContact: "Mom",
            EmergencyPhone: "07999888777",
            EmergencyContact2: null,
            EmergencyPhone2: null,
            TeamIds: new List<Guid> { team1.Id, team2.Id }
        );

        // Act
        var result = await service.CreatePlayerAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Teams);
        Assert.Equal(2, result.Teams.Count);
        Assert.Contains(result.Teams, t => t.Name == "Stripes");
        Assert.Contains(result.Teams, t => t.Name == "Hoops");
        Assert.NotNull(result.TeamIds);
        Assert.Equal(2, result.TeamIds.Count);
        Assert.Equal(team1.Id, result.TeamId);
    }

    [Fact]
    public async Task UpdatePlayerAsync_WithMultipleTeams_UpdatesAssignments()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);
        var team1 = new Team { Id = Guid.NewGuid(), Name = "Stripes", ColorHex = "#006837" };
        var team2 = new Team { Id = Guid.NewGuid(), Name = "Hoops", ColorHex = "#F59E0B" };
        dbContext.Teams.AddRange(team1, team2);

        var player = new Player
        {
            Id = Guid.NewGuid(),
            FirstName = "Bukayo",
            LastName = "Saka",
            IsActive = true
        };
        dbContext.Players.Add(player);
        dbContext.PlayerTeams.Add(new PlayerTeam { PlayerId = player.Id, TeamId = team1.Id });
        await dbContext.SaveChangesAsync();

        var service = new PlayerService(dbContext);
        var request = new UpdatePlayerRequest(
            FirstName: "Bukayo",
            LastName: "Saka",
            DateOfBirth: null,
            MedicalNotes: null,
            EmergencyContact: null,
            EmergencyPhone: null,
            EmergencyContact2: null,
            EmergencyPhone2: null,
            IsActive: true,
            SubscriptionStatus: "Active",
            TeamIds: new List<Guid> { team2.Id }
        );

        // Act
        var result = await service.UpdatePlayerAsync(player.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Teams);
        Assert.Single(result.Teams);
        Assert.Equal("Hoops", result.Teams[0].Name);
        Assert.Equal(team2.Id, result.TeamId);
    }

    [Fact]
    public async Task UpdatePlayerAsync_PreservesExistingTrainingCards_WhenNotProvidedInRequest()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);
        var player = new Player
        {
            Id = Guid.NewGuid(),
            FirstName = "Phil",
            LastName = "Foden",
            TrainingCardsCount = 5,
            IsActive = true
        };
        dbContext.Players.Add(player);
        await dbContext.SaveChangesAsync();

        var service = new PlayerService(dbContext);
        var request = new UpdatePlayerRequest(
            FirstName: "Phil",
            LastName: "Foden",
            DateOfBirth: null,
            MedicalNotes: null,
            EmergencyContact: null,
            EmergencyPhone: null,
            EmergencyContact2: null,
            EmergencyPhone2: null,
            IsActive: true,
            SubscriptionStatus: "Active",
            TrainingCardsCount: null // Not provided in request
        );

        // Act
        var result = await service.UpdatePlayerAsync(player.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.TrainingCardsCount);
        var inDb = await dbContext.Players.FindAsync(player.Id);
        Assert.Equal(5, inDb!.TrainingCardsCount);
    }

    [Fact]
    public async Task CreatePlayerAsync_SavesSigningFeePaid()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);
        var service = new PlayerService(dbContext);

        var request = new CreatePlayerRequest(
            FirstName: "Declan",
            LastName: "Rice",
            DateOfBirth: null,
            MedicalNotes: null,
            EmergencyContact: null,
            EmergencyPhone: null,
            EmergencyContact2: null,
            EmergencyPhone2: null,
            SigningFeePaid: true
        );

        // Act
        var result = await service.CreatePlayerAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.SigningFeePaid);
        var inDb = await dbContext.Players.FindAsync(result.Id);
        Assert.True(inDb!.SigningFeePaid);
    }

    [Fact]
    public async Task UpdateSigningFeeAsync_TogglesSigningFeePaid()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);
        var player = new Player
        {
            Id = Guid.NewGuid(),
            FirstName = "Jude",
            LastName = "Bellingham",
            SigningFeePaid = false
        };
        dbContext.Players.Add(player);
        await dbContext.SaveChangesAsync();

        var service = new PlayerService(dbContext);

        // Act
        var result = await service.UpdateSigningFeeAsync(player.Id, true);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.SigningFeePaid);
        var inDb = await dbContext.Players.FindAsync(player.Id);
        Assert.True(inDb!.SigningFeePaid);
    }
}
