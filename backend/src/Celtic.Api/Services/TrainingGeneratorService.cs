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
        var trainingService = scope.ServiceProvider.GetRequiredService<ITrainingService>();
        await trainingService.GenerateTrainingSessionsAsync();
    }
}
