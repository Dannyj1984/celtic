using System;
using System.Linq;
using System.Threading.Tasks;
using Celtic.Api.Data;
using Celtic.Api.Models;
using Celtic.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Celtic.UnitTests.Services;

public class TrainingServiceTests
{
    private CelticDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CelticDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CelticDbContext(options);
    }

    [Fact]
    public async Task GenerateTrainingSessionsAsync_CleansUpOutdatedSessions_AndCreatesNewSessions()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);

        // 1. Settings set to Wednesday 18:00
        var settings = new ClubSettings
        {
            TrainingDay = DayOfWeek.Wednesday,
            TrainingStartTime = new TimeSpan(18, 0, 0),
            TrainingEndTime = new TimeSpan(19, 0, 0),
            TrainingLocation = "Riverside Pitch"
        };
        dbContext.ClubSettings.Add(settings);

        // 2. Old stale training event on Tuesday in DB (future date)
        var staleTuesdayEvent = new Event
        {
            Id = Guid.NewGuid(),
            Type = "Training",
            DateTime = DateTime.UtcNow.AddDays(7), // future
            Location = "Riverside Pitch",
            Notes = "Regular training session"
        };
        dbContext.Events.Add(staleTuesdayEvent);
        await dbContext.SaveChangesAsync();

        var service = new TrainingService(dbContext, NullLogger<TrainingService>.Instance);

        // Act
        await service.GenerateTrainingSessionsAsync();

        // Assert
        var futureEvents = await dbContext.Events
            .Where(e => e.Type == "Training" && e.DateTime > DateTime.UtcNow)
            .ToListAsync();

        // Check that old stale event was removed
        Assert.DoesNotContain(futureEvents, e => e.Id == staleTuesdayEvent.Id);

        // Check that all future training events are on Wednesday
        Assert.Equal(4, futureEvents.Count);
        Assert.All(futureEvents, e => Assert.Equal(DayOfWeek.Wednesday, e.DateTime.DayOfWeek));
    }

    [Fact]
    public async Task GenerateTrainingSessionsAsync_PreservesRegisteredPlayers_WhenLocationChanges()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);

        var settings = new ClubSettings
        {
            TrainingDay = DayOfWeek.Wednesday,
            TrainingStartTime = new TimeSpan(18, 0, 0),
            TrainingEndTime = new TimeSpan(19, 0, 0),
            TrainingLocation = "Old Pitch Location"
        };
        dbContext.ClubSettings.Add(settings);

        var now = DateTime.UtcNow;
        var validStartTimes = TrainingService.GetUpcomingTrainingStartTimes(settings, now);
        var nextWednesday = validStartTimes.First();

        var player = new Player { Id = Guid.NewGuid(), FirstName = "Paul", LastName = "Scholes", IsActive = true };
        dbContext.Players.Add(player);

        var existingSession = new Event
        {
            Id = Guid.NewGuid(),
            Type = "Training",
            DateTime = nextWednesday,
            Location = "Old Pitch Location",
            Notes = "Regular training session"
        };
        dbContext.Events.Add(existingSession);

        dbContext.EventResponses.Add(new EventResponse
        {
            Id = Guid.NewGuid(),
            EventId = existingSession.Id,
            PlayerId = player.Id,
            Status = "Attending"
        });

        await dbContext.SaveChangesAsync();

        // Admin updates location in settings
        settings.TrainingLocation = "New Shiny Pitch 1";
        var service = new TrainingService(dbContext, NullLogger<TrainingService>.Instance);

        // Act
        await service.GenerateTrainingSessionsAsync();

        // Assert
        var updatedSession = await dbContext.Events
            .Include(e => e.Responses)
            .FirstOrDefaultAsync(e => e.Id == existingSession.Id);

        Assert.NotNull(updatedSession);
        Assert.Equal("New Shiny Pitch 1", updatedSession.Location);
        Assert.Single(updatedSession.Responses);
        Assert.Equal(player.Id, updatedSession.Responses.First().PlayerId);
    }
}
