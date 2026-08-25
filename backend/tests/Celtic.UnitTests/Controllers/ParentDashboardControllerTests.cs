using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Celtic.Api.Controllers;
using Celtic.Api.Data;
using Celtic.Api.Models;
using Celtic.Api.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Celtic.UnitTests.Controllers;

public class ParentDashboardControllerTests
{
    private CelticDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CelticDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CelticDbContext(options);
    }

    private ParentDashboardController CreateController(CelticDbContext dbContext, string userId)
    {
        var controller = new ParentDashboardController(dbContext);
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));
        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = userClaims }
        };
        return controller;
    }

    [Fact]
    public async Task GetDashboard_ReturnsOk_WithDashboardData()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);
        
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser { Id = userId, UserName = "parent@test.com", FullName = "Alex" };
        var player = new Player { Id = Guid.NewGuid(), FirstName = "Leo", LastName = "Messi", SubscriptionStatus = "Active" };
        
        dbContext.Users.Add(user);
        dbContext.Players.Add(player);
        dbContext.PlayerParents.Add(new PlayerParent { UserId = userId, PlayerId = player.Id });

        dbContext.ClubSettings.Add(new ClubSettings 
        { 
            NextSubPaymentDate = new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc),
            TrainingDay = DayOfWeek.Wednesday,
            TrainingStartTime = new TimeSpan(17, 30, 0),
            TrainingEndTime = new TimeSpan(19, 0, 0),
            TrainingLocation = "Riverside Sports Complex",
            CoachWhatsAppNumber = "1234567890"
        });

        var season = new Season { Id = Guid.NewGuid(), Name = "2026 Season", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddYears(1) };
        dbContext.Seasons.Add(season);

        var nextMatch = new Match { Id = Guid.NewGuid(), SeasonId = season.Id, Date = DateTime.UtcNow.AddDays(2), Opposition = "JNR Tigers", Location = "Riverside Pitch 4" };
        dbContext.Matches.Add(nextMatch);
        dbContext.Events.Add(new Event { Id = Guid.NewGuid(), SeasonId = season.Id, Type = "Match", DateTime = nextMatch.Date, Location = nextMatch.Location, MatchId = nextMatch.Id, Match = nextMatch });

        // Add 20 training events in the past, Leo attended 18
        for (int i = 0; i < 20; i++)
        {
            var evt = new Event { Id = Guid.NewGuid(), SeasonId = season.Id, Type = "Training", DateTime = DateTime.UtcNow.AddDays(-i - 1) };
            dbContext.Events.Add(evt);
            
            if (i < 18)
            {
                dbContext.EventResponses.Add(new EventResponse { Id = Guid.NewGuid(), EventId = evt.Id, PlayerId = player.Id, Status = "Attending" });
            }
        }

        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext, userId);

        // Act
        var result = await controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = okResult.Value as DashboardDto;
        Assert.NotNull(dto);

        Assert.Equal("Alex", dto.ParentName);
        Assert.Equal("Leo Messi", dto.PlayerName);
        Assert.Equal("Active", dto.SubscriptionStatus);
        Assert.Equal(new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc), dto.NextSubPaymentDate);
        
        Assert.NotNull(dto.NextMatch);
        Assert.Equal("JNR Tigers", dto.NextMatch.Opposition);
        
        Assert.NotNull(dto.TrainingSchedule);
        Assert.Equal("Wednesday", dto.TrainingSchedule.Day);
        
        Assert.NotNull(dto.Performance);
        Assert.Equal(20, dto.Performance.Training.TotalSessions);
        Assert.Equal(18, dto.Performance.Training.AttendedSessions);
        Assert.Equal(90.0, dto.Performance.Training.Percentage);
        
        Assert.Equal(0, dto.Performance.Matches.TotalSessions);
        Assert.Equal(0, dto.Performance.Matches.AttendedSessions);
    }

    [Fact]
    public async Task GetDashboard_IncludesCoachNotes_WhenPresent()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);
        
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser { Id = userId, UserName = "parent@test.com", FullName = "Alex" };
        var player = new Player 
        { 
            Id = Guid.NewGuid(), 
            FirstName = "Leo", 
            LastName = "Messi", 
            SubscriptionStatus = "Active",
            CoachNotes = "Great focus in training today!"
        };
        
        dbContext.Users.Add(user);
        dbContext.Players.Add(player);
        dbContext.PlayerParents.Add(new PlayerParent { UserId = userId, PlayerId = player.Id });
        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext, userId);

        // Act
        var result = await controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = okResult.Value as DashboardDto;
        Assert.NotNull(dto);
        Assert.Equal("Great focus in training today!", dto.CoachNotes);
    }

    [Fact]
    public async Task GetDashboard_IncludesCardsProgress_WhenPresent()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);

        var settings = new ClubSettings
        {
            TrainingDay = DayOfWeek.Wednesday,
            TrainingStartTime = new TimeSpan(18, 0, 0),
            TrainingEndTime = new TimeSpan(19, 0, 0),
            CardRewardsJson = "[{\"cardsRequired\":5,\"rewardText\":\"Choose game in next session\"},{\"cardsRequired\":10,\"rewardText\":\"Pick captain\"}]"
        };
        dbContext.ClubSettings.Add(settings);

        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser { Id = userId, UserName = "parent@test.com", FullName = "Alex" };
        var player = new Player
        {
            Id = Guid.NewGuid(),
            FirstName = "Leo",
            LastName = "Messi",
            SubscriptionStatus = "Active",
            TrainingCardsCount = 3
        };

        dbContext.Users.Add(user);
        dbContext.Players.Add(player);
        dbContext.PlayerParents.Add(new PlayerParent { UserId = userId, PlayerId = player.Id });
        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext, userId);

        // Act
        var result = await controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = okResult.Value as DashboardDto;
        Assert.NotNull(dto);
        Assert.NotNull(dto.CardsProgress);
        Assert.Equal(3, dto.CardsProgress.CardsCount);
        Assert.NotNull(dto.CardsProgress.NextReward);
        Assert.Equal(5, dto.CardsProgress.NextReward.CardsRequired);
        Assert.Equal("Choose game in next session", dto.CardsProgress.NextReward.RewardText);
        Assert.Equal(2, dto.CardsProgress.CardsUntilNextReward);
    }

    [Fact]
    public async Task RegisterForTraining_ReturnsOk_CreatesEventResponse()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);
        var userId = Guid.NewGuid().ToString();
        var player = new Player { Id = Guid.NewGuid(), FirstName = "Leo", LastName = "Messi" };
        
        dbContext.Users.Add(new ApplicationUser { Id = userId });
        dbContext.Players.Add(player);
        dbContext.PlayerParents.Add(new PlayerParent { UserId = userId, PlayerId = player.Id });

        var nextTraining = new Event { Id = Guid.NewGuid(), Type = "Training", DateTime = DateTime.UtcNow.AddDays(2) };
        dbContext.Events.Add(nextTraining);
        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext, userId);

        // Act
        var result = await controller.RegisterForTraining();

        // Assert
        Assert.IsType<OkResult>(result);
        var response = await dbContext.EventResponses.FirstOrDefaultAsync();
        Assert.NotNull(response);
        Assert.Equal("Attending", response.Status);
        Assert.Equal(nextTraining.Id, response.EventId);
        Assert.Equal(player.Id, response.PlayerId);
        Assert.Equal(userId, response.RespondedByUserId);
    }

    [Fact]
    public async Task ConfirmMatchAvailability_ReturnsOk_CreatesEventResponse()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);
        var userId = Guid.NewGuid().ToString();
        var player = new Player { Id = Guid.NewGuid(), FirstName = "Leo", LastName = "Messi" };
        
        dbContext.Users.Add(new ApplicationUser { Id = userId });
        dbContext.Players.Add(player);
        dbContext.PlayerParents.Add(new PlayerParent { UserId = userId, PlayerId = player.Id });

        var nextMatchEvent = new Event { Id = Guid.NewGuid(), Type = "Match", DateTime = DateTime.UtcNow.AddDays(2) };
        dbContext.Events.Add(nextMatchEvent);
        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext, userId);

        // Act
        var result = await controller.ConfirmMatchAvailability();

        // Assert
        Assert.IsType<OkResult>(result);
        var response = await dbContext.EventResponses.FirstOrDefaultAsync();
        Assert.NotNull(response);
        Assert.Equal("Attending", response.Status);
        Assert.Equal(nextMatchEvent.Id, response.EventId);
        Assert.Equal(player.Id, response.PlayerId);
        Assert.Equal(userId, response.RespondedByUserId);
    }

    [Fact]
    public async Task UpdateKitSizing_UpdatesPlayerKitSizes()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);
        var userId = Guid.NewGuid().ToString();
        var player = new Player { Id = Guid.NewGuid(), FirstName = "Leo", LastName = "Messi", ShirtSize = "5-6 yrs", ShortSize = "5-6 yrs", SockSize = 10 };
        
        dbContext.Users.Add(new ApplicationUser { Id = userId });
        dbContext.Players.Add(player);
        dbContext.PlayerParents.Add(new PlayerParent { UserId = userId, PlayerId = player.Id });
        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext, userId);
        var request = new UpdateKitSizingDto { ShirtSize = "7-8 yrs", ShortSize = "7-8 yrs", SockSize = 12 };

        // Act
        var result = await controller.UpdateKitSizing(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var updatedPlayer = await dbContext.Players.FindAsync(player.Id);
        Assert.NotNull(updatedPlayer);
        Assert.Equal("7-8 yrs", updatedPlayer.ShirtSize);
        Assert.Equal("7-8 yrs", updatedPlayer.ShortSize);
        Assert.Equal(12, updatedPlayer.SockSize);
    }

    [Fact]
    public async Task UpdateDateOfBirth_UpdatesPlayerDateOfBirth()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);
        var userId = Guid.NewGuid().ToString();
        var player = new Player { Id = Guid.NewGuid(), FirstName = "Leo", LastName = "Messi", DateOfBirth = new DateTime(2018, 1, 1) };
        
        dbContext.Users.Add(new ApplicationUser { Id = userId });
        dbContext.Players.Add(player);
        dbContext.PlayerParents.Add(new PlayerParent { UserId = userId, PlayerId = player.Id });
        await dbContext.SaveChangesAsync();

        var controller = CreateController(dbContext, userId);
        var expectedDob = new DateTime(2019, 6, 15);
        var request = new UpdateDateOfBirthDto { DateOfBirth = expectedDob };

        // Act
        var result = await controller.UpdateDateOfBirth(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var updatedPlayer = await dbContext.Players.FindAsync(player.Id);
        Assert.NotNull(updatedPlayer);
        Assert.Equal(expectedDob, updatedPlayer.DateOfBirth);
    }
}
