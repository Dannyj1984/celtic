using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Celtic.Api.Data;
using Celtic.Api.Models;
using Celtic.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Celtic.UnitTests.Services;

public class EventServiceTests
{
    private CelticDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CelticDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CelticDbContext(options);
    }

    [Fact]
    public async Task UpdateEventAttendanceAsync_UpdatesExistingAndAddsNewResponses()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);

        var adminUser = new ApplicationUser
        {
            Id = "admin-1",
            UserName = "admin@celtic.app",
            Email = "admin@celtic.app",
            FullName = "Admin User"
        };
        dbContext.Users.Add(adminUser);

        var player1 = new Player { Id = Guid.NewGuid(), FirstName = "Tom", LastName = "Brady", IsActive = true };
        var player2 = new Player { Id = Guid.NewGuid(), FirstName = "Jerry", LastName = "Rice", IsActive = true };
        dbContext.Players.AddRange(player1, player2);

        var evt = new Event
        {
            Id = Guid.NewGuid(),
            Type = "Training",
            DateTime = DateTime.UtcNow,
            Location = "Main Pitch"
        };
        dbContext.Events.Add(evt);

        // Pre-existing response for player1
        dbContext.EventResponses.Add(new EventResponse
        {
            Id = Guid.NewGuid(),
            EventId = evt.Id,
            PlayerId = player1.Id,
            Status = "NotAttending",
            RespondedByUserId = adminUser.Id
        });

        await dbContext.SaveChangesAsync();

        var service = new EventService(dbContext);

        // Act - Mark both player1 and player2 as attending
        var updated = await service.UpdateEventAttendanceAsync(evt.Id, new List<Guid> { player1.Id, player2.Id }, adminUser.Id);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(2, updated.AttendingPlayers.Count);
        Assert.Contains(updated.AttendingPlayers, p => p.PlayerId == player1.Id);
        Assert.Contains(updated.AttendingPlayers, p => p.PlayerId == player2.Id);
    }

    [Fact]
    public async Task UpdateEventAttendanceAsync_SetsCaptainsCorrectly()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);

        var adminUser = new ApplicationUser
        {
            Id = "admin-1",
            UserName = "admin@celtic.app",
            Email = "admin@celtic.app",
            FullName = "Admin User"
        };
        dbContext.Users.Add(adminUser);

        var player1 = new Player { Id = Guid.NewGuid(), FirstName = "John", LastName = "Terry", IsActive = true };
        var player2 = new Player { Id = Guid.NewGuid(), FirstName = "Frank", LastName = "Lampard", IsActive = true };
        dbContext.Players.AddRange(player1, player2);

        var evt = new Event
        {
            Id = Guid.NewGuid(),
            Type = "Training",
            DateTime = DateTime.UtcNow,
            Location = "Main Pitch"
        };
        dbContext.Events.Add(evt);
        await dbContext.SaveChangesAsync();

        var service = new EventService(dbContext);

        // Act - Set player1 as Captain 1 and player2 as Captain 2
        var updated = await service.UpdateEventAttendanceAsync(
            evt.Id, 
            new List<Guid> { player1.Id, player2.Id }, 
            adminUser.Id, 
            player1.Id, 
            player2.Id
        );

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(player1.Id, updated.Captain1PlayerId);
        Assert.Equal("John Terry", updated.Captain1PlayerName);
        Assert.Equal(player2.Id, updated.Captain2PlayerId);
        Assert.Equal("Frank Lampard", updated.Captain2PlayerName);
    }

    [Fact]
    public async Task DeleteEventAsync_RemovesEventAndResponses()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);

        var player = new Player { Id = Guid.NewGuid(), FirstName = "Frank", LastName = "Lampard", IsActive = true };
        dbContext.Players.Add(player);

        var evt = new Event
        {
            Id = Guid.NewGuid(),
            Type = "Training",
            DateTime = DateTime.UtcNow.AddDays(-3),
            Location = "Training Complex"
        };
        dbContext.Events.Add(evt);

        dbContext.EventResponses.Add(new EventResponse
        {
            Id = Guid.NewGuid(),
            EventId = evt.Id,
            PlayerId = player.Id,
            Status = "Attending"
        });

        await dbContext.SaveChangesAsync();

        var service = new EventService(dbContext);

        // Act
        await service.DeleteEventAsync(evt.Id);

        // Assert
        var deletedEvent = await dbContext.Events.FindAsync(evt.Id);
        Assert.Null(deletedEvent);

        var remainingResponses = await dbContext.EventResponses.Where(r => r.EventId == evt.Id).ToListAsync();
        Assert.Empty(remainingResponses);
    }
}
