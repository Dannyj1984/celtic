using Microsoft.EntityFrameworkCore;
using Celtic.Api.Data;
using Celtic.Api.Models;

namespace Celtic.Api.Services;

public class TrainingGeneratorService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<TrainingGeneratorService> _logger;

    public TrainingGeneratorService(IServiceProvider services, ILogger<TrainingGeneratorService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Training Generator Service is starting.");

        // Initial run
        await GenerateTrainingSessionsAsync();

        // Run every 24 hours
        using PeriodicTimer timer = new(TimeSpan.FromDays(1));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await GenerateTrainingSessionsAsync();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Training Generator Service is stopping.");
        }
    }

    private async Task GenerateTrainingSessionsAsync()
    {
        _logger.LogInformation("Checking for upcoming training sessions...");

        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CelticDbContext>();

        var settings = await db.ClubSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            _logger.LogWarning("Club settings not found. Skipping training generation.");
            return;
        }

        var currentSeason = await db.Seasons.FirstOrDefaultAsync(s => s.IsCurrent);
        if (currentSeason == null)
        {
            _logger.LogWarning("No current season found. Training sessions will not be linked to a season.");
        }

        // We want to ensure training exists for the next 4 weeks
        var today = DateTime.UtcNow.Date;
        var trainingDay = settings.TrainingDay;
        
        for (int i = 0; i < 4; i++)
        {
            // Find the i-th occurrence of the training day starting from today
            var date = GetNextWeekday(today.AddDays(i * 7), trainingDay);
            var startTime = date.Add(settings.TrainingStartTime);

            // Check if a training session already exists for this exact time
            var exists = await db.Events.AnyAsync(e => 
                e.Type == "Training" && 
                e.DateTime == startTime);

            if (!exists)
            {
                _logger.LogInformation("Generating training session for {Date}", startTime);
                var training = new Event
                {
                    SeasonId = currentSeason?.Id,
                    Type = "Training",
                    DateTime = startTime,
                    Location = settings.TrainingLocation,
                    Notes = "Regular training session"
                };
                db.Events.Add(training);
            }
        }

        await db.SaveChangesAsync();
    }

    private static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
    {
        int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
        return start.AddDays(daysToAdd);
    }
}
