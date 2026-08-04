using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Celtic.Api.Data;
using Celtic.Api.Models;

namespace Celtic.Api.Services;

public class TrainingService : ITrainingService
{
    private readonly CelticDbContext _db;
    private readonly ILogger<TrainingService> _logger;

    public static readonly TimeZoneInfo UkTimeZone = GetUkTimeZone();

    private static TimeZoneInfo GetUkTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        }
        catch
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        }
    }

    public TrainingService(CelticDbContext db, ILogger<TrainingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task GenerateTrainingSessionsAsync()
    {
        var settings = await _db.ClubSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            _logger.LogWarning("Club settings not found. Skipping training generation.");
            return;
        }

        var now = DateTime.UtcNow;
        var validStartTimes = GetUpcomingTrainingStartTimes(settings, now);

        // Find future regular training events in DB (either marked as regular or with empty notes)
        var futureTrainingEvents = await _db.Events
            .Where(e => e.Type == "Training" && e.DateTime > now && (e.Notes == "Regular training session" || string.IsNullOrEmpty(e.Notes)))
            .ToListAsync();

        // Purge any outdated regular training sessions on wrong day, time, or location
        foreach (var evt in futureTrainingEvents)
        {
            if (!validStartTimes.Contains(evt.DateTime) || evt.Location != settings.TrainingLocation)
            {
                _logger.LogInformation("Removing outdated training session: {Date}", evt.DateTime);
                _db.Events.Remove(evt);
            }
        }

        var currentSeason = await _db.Seasons.FirstOrDefaultAsync(s => s.IsCurrent);

        // Ensure all valid upcoming sessions exist
        foreach (var startTime in validStartTimes)
        {
            var exists = await _db.Events.AnyAsync(e => e.Type == "Training" && e.DateTime == startTime);
            if (!exists)
            {
                _logger.LogInformation("Generating training session for {Date}", startTime);
                _db.Events.Add(new Event
                {
                    SeasonId = currentSeason?.Id,
                    Type = "Training",
                    DateTime = startTime,
                    Location = settings.TrainingLocation,
                    Notes = "Regular training session"
                });
            }
        }

        await _db.SaveChangesAsync();
    }

    public static List<DateTime> GetUpcomingTrainingStartTimes(ClubSettings settings, DateTime now)
    {
        var validStartTimes = new List<DateTime>();
        var ukNow = TimeZoneInfo.ConvertTimeFromUtc(now, UkTimeZone);
        var ukToday = ukNow.Date;

        var firstDate = GetNextWeekday(ukToday, settings.TrainingDay);
        var firstLocalStartTime = DateTime.SpecifyKind(firstDate.Add(settings.TrainingStartTime), DateTimeKind.Unspecified);
        var firstUtcStartTime = TimeZoneInfo.ConvertTimeToUtc(firstLocalStartTime, UkTimeZone);

        // If the calculated first session for today has already passed, move to next week's session
        if (firstUtcStartTime <= now)
        {
            firstDate = GetNextWeekday(ukToday.AddDays(1), settings.TrainingDay);
        }

        for (int i = 0; i < 4; i++)
        {
            var localDate = firstDate.AddDays(i * 7);
            var localDateTime = DateTime.SpecifyKind(localDate.Add(settings.TrainingStartTime), DateTimeKind.Unspecified);
            var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, UkTimeZone);
            validStartTimes.Add(utcDateTime);
        }

        return validStartTimes;
    }

    private static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
    {
        int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
        return start.AddDays(daysToAdd);
    }
}
