using System;
using System.Linq;
using System.Threading.Tasks;
using Celtic.Api.Data;
using Celtic.Api.DTOs;
using Celtic.Api.Models;
using Celtic.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Celtic.UnitTests.Services;

public class PaymentServiceTests
{
    private CelticDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CelticDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CelticDbContext(options);
    }

    [Fact]
    public async Task GetFinancialSummaryAsync_CalculatesTotalsCorrectly()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);

        var season = new Season
        {
            Id = Guid.NewGuid(),
            Name = "2026-27",
            StartDate = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            SubAmount = 30m,
            SubFrequency = "Monthly",
            IsCurrent = true
        };
        dbContext.Seasons.Add(season);

        var player1 = new Player { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", IsActive = true };
        var player2 = new Player { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", IsActive = true };
        dbContext.Players.AddRange(player1, player2);

        var payment1 = new SubPayment
        {
            Id = Guid.NewGuid(),
            PlayerId = player1.Id,
            SeasonId = season.Id,
            Amount = 30m,
            PaidDate = DateTime.UtcNow,
            PeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            PeriodEnd = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 28, 0, 0, 0, DateTimeKind.Utc),
            Method = "BankTransfer"
        };
        dbContext.SubPayments.Add(payment1);

        var expense1 = new Expense
        {
            Id = Guid.NewGuid(),
            SeasonId = season.Id,
            Category = "PitchHire",
            Description = "August Pitch Rental",
            Amount = 100m,
            Date = DateTime.UtcNow
        };
        dbContext.Expenses.Add(expense1);

        await dbContext.SaveChangesAsync();

        var service = new PaymentService(dbContext);

        // Act
        var summary = await service.GetFinancialSummaryAsync(season.Id);

        // Assert
        Assert.Equal(season.Id, summary.SeasonId);
        Assert.Equal(30m, summary.TotalIncome);
        Assert.Equal(100m, summary.TotalExpenses);
        Assert.Equal(-70m, summary.NetBalance);
        Assert.Equal(2, summary.ActivePlayersCount);
        Assert.Equal(1, summary.CurrentMonthPaidCount);
    }

    [Fact]
    public async Task RecordSubPaymentAsync_CreatesNewPayment_OrUpdatesExisting()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);

        var season = new Season
        {
            Id = Guid.NewGuid(),
            Name = "2026-27",
            StartDate = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            SubAmount = 25m,
            SubFrequency = "Monthly"
        };
        var player = new Player { Id = Guid.NewGuid(), FirstName = "Alex", LastName = "Ferguson", IsActive = true };
        dbContext.Seasons.Add(season);
        dbContext.Players.Add(player);
        await dbContext.SaveChangesAsync();

        var service = new PaymentService(dbContext);

        var request = new RecordSubPaymentRequest
        {
            PlayerId = player.Id,
            SeasonId = season.Id,
            Amount = 25m,
            PaidDate = DateTime.UtcNow,
            PeriodStart = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            PeriodEnd = new DateTime(2026, 8, 31, 23, 59, 59, DateTimeKind.Utc),
            Method = "Cash",
            Notes = "First month sub"
        };

        // Act
        var result = await service.RecordSubPaymentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(25m, result.Amount);
        Assert.Equal("Cash", result.Method);

        // Verify in DB
        var dbPayment = await dbContext.SubPayments.FirstOrDefaultAsync(p => p.Id == result.Id);
        Assert.NotNull(dbPayment);
        Assert.Equal("First month sub", dbPayment.Notes);
    }

    [Fact]
    public async Task CreateExpenseAsync_And_DeleteExpenseAsync_WorkCorrectly()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);

        var season = new Season
        {
            Id = Guid.NewGuid(),
            Name = "2026-27",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(9)
        };
        dbContext.Seasons.Add(season);
        await dbContext.SaveChangesAsync();

        var service = new PaymentService(dbContext);

        var request = new CreateExpenseRequest
        {
            SeasonId = season.Id,
            Category = "Kit",
            Description = "New match balls and bibs",
            Amount = 150m,
            Date = DateTime.UtcNow,
            PaidBy = "Coach Smith"
        };

        // Act - Create
        var created = await service.CreateExpenseAsync(request);
        Assert.NotNull(created);
        Assert.Equal(150m, created.Amount);
        Assert.Equal("Kit", created.Category);

        var expenses = await service.GetExpensesAsync(season.Id);
        Assert.Single(expenses);

        // Act - Delete
        var deleted = await service.DeleteExpenseAsync(created.Id);
        Assert.True(deleted);

        var expensesAfterDelete = await service.GetExpensesAsync(season.Id);
        Assert.Empty(expensesAfterDelete);
    }

    [Fact]
    public async Task GetPlayerSubStatusesAsync_IncludesTeamDetails()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var dbContext = GetDbContext(dbName);

        var season = new Season
        {
            Id = Guid.NewGuid(),
            Name = "2026-27",
            StartDate = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            SubAmount = 30m,
            SubFrequency = "Monthly"
        };
        dbContext.Seasons.Add(season);

        var team = new Team { Id = Guid.NewGuid(), Name = "Hoops", ColorHex = "#F59E0B" };
        dbContext.Teams.Add(team);

        var player = new Player
        {
            Id = Guid.NewGuid(),
            FirstName = "Leo",
            LastName = "Messi",
            IsActive = true,
            TeamId = team.Id,
            Team = team
        };
        dbContext.Players.Add(player);
        await dbContext.SaveChangesAsync();

        var service = new PaymentService(dbContext);

        // Act
        var statuses = await service.GetPlayerSubStatusesAsync(season.Id, 2026, 8);

        // Assert
        Assert.Single(statuses);
        var pStatus = statuses.First();
        Assert.Equal(team.Id, pStatus.TeamId);
        Assert.Equal("Hoops", pStatus.TeamName);
    }
}
