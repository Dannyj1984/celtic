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
}
