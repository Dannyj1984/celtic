using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Celtic.Api.Controllers;
using Celtic.Api.Data;
using Celtic.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Celtic.UnitTests.Controllers;

public class SettingsControllerTests
{
    private CelticDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CelticDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CelticDbContext(options);
    }

    private SettingsController CreateController(CelticDbContext dbContext)
    {
        var trainingService = new Celtic.Api.Services.TrainingService(dbContext, Microsoft.Extensions.Logging.Abstractions.NullLogger<Celtic.Api.Services.TrainingService>.Instance);
        var controller = new SettingsController(dbContext, trainingService);
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));
        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = userClaims }
        };
        return controller;
    }

    [Fact]
    public async Task GetSettings_ReturnsSettings()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);
        
        var settings = new ClubSettings 
        { 
            NextSubPaymentDate = new DateTime(2026, 11, 1),
            TrainingDay = DayOfWeek.Monday,
            TrainingStartTime = new TimeSpan(18, 0, 0),
            TrainingEndTime = new TimeSpan(19, 30, 0),
            TrainingLocation = "Main Field",
            CoachWhatsAppNumber = "987654321"
        };
        dbContext.ClubSettings.Add(settings);
        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext);

        // Act
        var result = await controller.GetSettings();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedSettings = Assert.IsType<ClubSettings>(okResult.Value);
        Assert.Equal(DayOfWeek.Monday, returnedSettings.TrainingDay);
    }

    [Fact]
    public async Task UpdateSettings_CreatesOrUpdatesSettings()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);
        var controller = CreateController(dbContext);

        var newSettings = new ClubSettings 
        { 
            NextSubPaymentDate = new DateTime(2026, 12, 1),
            TrainingDay = DayOfWeek.Friday,
            TrainingStartTime = new TimeSpan(18, 0, 0),
            TrainingEndTime = new TimeSpan(19, 0, 0),
            TrainingLocation = "Indoor Arena",
            CoachWhatsAppNumber = "111222333"
        };

        // Act
        var result = await controller.UpdateSettings(newSettings);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var savedSettings = await dbContext.ClubSettings.FirstOrDefaultAsync();
        Assert.NotNull(savedSettings);
        Assert.Equal(DayOfWeek.Friday, savedSettings.TrainingDay);
        Assert.Equal("Indoor Arena", savedSettings.TrainingLocation);
    }
}
