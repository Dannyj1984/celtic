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
}
