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
}
