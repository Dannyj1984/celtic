using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Celtic.Api.Data;
using Celtic.Api.DTOs;
using Celtic.Api.Models;
using Celtic.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Celtic.UnitTests.Services;

public class MatchSquadServiceTests
{
    private CelticDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CelticDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CelticDbContext(options);
    }

    private List<Player> CreatePlayers(CelticDbContext context, int count)
    {
        var players = new List<Player>();
        for (int i = 1; i <= count; i++)
        {
            var p = new Player
            {
                Id = Guid.NewGuid(),
                FirstName = $"Player{i}",
                LastName = "Test",
                IsActive = true
            };
            context.Players.Add(p);
            players.Add(p);
        }
        return players;
    }

    [Fact]
    public async Task GenerateSquad_With6Players_Produces1SubPerInterval_AndEqualPlayingTime()
    {
        // Arrange (6 players total -> 5 on pitch, 1 on bench => 1 sub per interval)
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var players = CreatePlayers(context, 6);
        await context.SaveChangesAsync();

        var service = new MatchSquadService(context);

        // Act
        var squad = await service.GenerateSquadAsync(new GenerateMatchSquadRequest
        {
            CustomPlayerIds = players.Select(p => p.Id).ToList(),
            TotalPeriods = 6,
            PeriodDurationMinutes = 6
        });

        // Assert
        Assert.Equal(6, squad.Periods.Count);
        Assert.Equal(6, squad.RegisteredPlayers.Count);

        // Period 1 is starting lineup (0 subs)
        Assert.Empty(squad.Periods[0].Substitutions);
        Assert.NotNull(squad.Periods[0].Goalkeeper);
        Assert.Equal(4, squad.Periods[0].OutfieldPlayers.Count);
        Assert.Single(squad.Periods[0].BenchPlayers);

        // Subsequent periods should each have 1 substitution
        for (int i = 1; i < 6; i++)
        {
            var period = squad.Periods[i];
            Assert.Single(period.Substitutions);
            Assert.NotNull(period.Goalkeeper);
            Assert.Equal(4, period.OutfieldPlayers.Count);
            Assert.Single(period.BenchPlayers);
        }

        // Check equal playing time: 6 players x 30 mins each = 180 total player minutes
        Assert.All(squad.PlayerMinutes, pm => Assert.Equal(30, pm.TotalMinutes));
    }

    [Fact]
    public async Task GenerateSquad_With7Players_Produces2SubsPerInterval()
    {
        // Arrange (7 players -> 5 on pitch, 2 on bench => 2 subs per interval)
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var players = CreatePlayers(context, 7);
        await context.SaveChangesAsync();

        var service = new MatchSquadService(context);

        // Act
        var squad = await service.GenerateSquadAsync(new GenerateMatchSquadRequest
        {
            CustomPlayerIds = players.Select(p => p.Id).ToList(),
            TotalPeriods = 6,
            PeriodDurationMinutes = 6
        });

        // Assert
        Assert.Equal(6, squad.Periods.Count);
        for (int i = 1; i < 6; i++)
        {
            var period = squad.Periods[i];
            Assert.Equal(2, period.Substitutions.Count);
            Assert.Equal(4, period.OutfieldPlayers.Count);
            Assert.Equal(2, period.BenchPlayers.Count);
        }

        // Total minutes should be well balanced (either 24 or 30 mins)
        Assert.All(squad.PlayerMinutes, pm => Assert.InRange(pm.TotalMinutes, 24, 30));
    }

    [Fact]
    public async Task GenerateSquad_With8Players_Produces3SubsPerInterval()
    {
        // Arrange (8 players -> 5 on pitch, 3 on bench => 3 subs per interval)
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var players = CreatePlayers(context, 8);
        await context.SaveChangesAsync();

        var service = new MatchSquadService(context);

        // Act
        var squad = await service.GenerateSquadAsync(new GenerateMatchSquadRequest
        {
            CustomPlayerIds = players.Select(p => p.Id).ToList(),
            TotalPeriods = 6,
            PeriodDurationMinutes = 6
        });

        // Assert
        Assert.Equal(6, squad.Periods.Count);
        for (int i = 1; i < 6; i++)
        {
            var period = squad.Periods[i];
            Assert.Equal(3, period.Substitutions.Count);
            Assert.Equal(4, period.OutfieldPlayers.Count);
            Assert.Equal(3, period.BenchPlayers.Count);
        }

        // Balanced minutes
        Assert.All(squad.PlayerMinutes, pm => Assert.InRange(pm.TotalMinutes, 18, 24));
    }

    [Fact]
    public async Task GenerateSquad_With5Players_Produces0Subs_AndFullMatchForEveryone()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var players = CreatePlayers(context, 5);
        await context.SaveChangesAsync();

        var service = new MatchSquadService(context);

        // Act
        var squad = await service.GenerateSquadAsync(new GenerateMatchSquadRequest
        {
            CustomPlayerIds = players.Select(p => p.Id).ToList()
        });

        // Assert
        Assert.All(squad.Periods, p => Assert.Empty(p.Substitutions));
        Assert.All(squad.Periods, p => Assert.Empty(p.BenchPlayers));
        Assert.All(squad.PlayerMinutes, pm => Assert.Equal(36, pm.TotalMinutes));
    }

    [Fact]
    public async Task GenerateSquad_SplitsGoalkeepers_OnePerHalf()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var players = CreatePlayers(context, 7);
        await context.SaveChangesAsync();

        var gk1 = players[0];
        var gk2 = players[1];

        var service = new MatchSquadService(context);

        // Act
        var squad = await service.GenerateSquadAsync(new GenerateMatchSquadRequest
        {
            CustomPlayerIds = players.Select(p => p.Id).ToList(),
            FirstHalfGoalkeeperPlayerId = gk1.Id,
            SecondHalfGoalkeeperPlayerId = gk2.Id,
            TotalPeriods = 6,
            PeriodDurationMinutes = 6
        });

        // Assert
        Assert.Equal(gk1.Id, squad.FirstHalfGoalkeeperPlayerId);
        Assert.Equal(gk2.Id, squad.SecondHalfGoalkeeperPlayerId);

        // Half 1 (periods 0, 1, 2) should have gk1
        Assert.Equal(gk1.Id, squad.Periods[0].Goalkeeper!.Id);
        Assert.Equal(gk1.Id, squad.Periods[1].Goalkeeper!.Id);
        Assert.Equal(gk1.Id, squad.Periods[2].Goalkeeper!.Id);

        // Half 2 (periods 3, 4, 5) should have gk2
        Assert.Equal(gk2.Id, squad.Periods[3].Goalkeeper!.Id);
        Assert.Equal(gk2.Id, squad.Periods[4].Goalkeeper!.Id);
        Assert.Equal(gk2.Id, squad.Periods[5].Goalkeeper!.Id);
    }

    [Fact]
    public async Task SaveSquad_And_GetSquad_PersistsAndRetrievesCorrectly()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var players = CreatePlayers(context, 6);

        var match = new Match
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            Opposition = "Rangers"
        };
        context.Matches.Add(match);
        await context.SaveChangesAsync();

        var service = new MatchSquadService(context);
        var generated = await service.GenerateSquadAsync(new GenerateMatchSquadRequest
        {
            MatchId = match.Id,
            CustomPlayerIds = players.Select(p => p.Id).ToList()
        });

        // Act
        var saved = await service.SaveSquadAsync(match.Id, null, new SaveMatchSquadRequest
        {
            MatchId = match.Id,
            FirstHalfGoalkeeperPlayerId = generated.FirstHalfGoalkeeperPlayerId,
            SecondHalfGoalkeeperPlayerId = generated.SecondHalfGoalkeeperPlayerId,
            TotalPeriods = generated.TotalPeriods,
            PeriodDurationMinutes = generated.PeriodDurationMinutes,
            Periods = generated.Periods,
            RegisteredPlayers = generated.RegisteredPlayers
        });

        var retrieved = await service.GetSquadByMatchIdAsync(match.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(match.Id, retrieved.MatchId);
        Assert.Equal(generated.Periods.Count, retrieved.Periods.Count);
        Assert.Equal(generated.FirstHalfGoalkeeperPlayerId, retrieved.FirstHalfGoalkeeperPlayerId);
    }

    [Fact]
    public async Task GenerateSquad_With2x15MinHalves_UsesHalfDurationAndGeneratesCorrectSchedule()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var players = CreatePlayers(context, 6);

        var match = new Match
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            Opposition = "Hyde United",
            HalfDurationMinutes = 15
        };
        context.Matches.Add(match);
        await context.SaveChangesAsync();

        var service = new MatchSquadService(context);

        // Act
        var squad = await service.GenerateSquadAsync(new GenerateMatchSquadRequest
        {
            MatchId = match.Id,
            CustomPlayerIds = players.Select(p => p.Id).ToList()
        });

        // Assert
        Assert.Equal(15, squad.HalfDurationMinutes);
        Assert.Equal(5, squad.PeriodDurationMinutes); // 15 / 5 = 3 periods per half -> 6 periods total (30m total)
        Assert.Equal(6, squad.TotalPeriods);
        Assert.Equal(30, squad.Periods.Last().EndMinute);
        Assert.All(squad.PlayerMinutes, pm => Assert.Equal(25, pm.TotalMinutes)); // 6 players x 25m = 150 player minutes (5 on pitch x 30m)
    }

    [Fact]
    public async Task GenerateSquad_With2x25MinHalves_UsesHalfDurationAndGeneratesCorrectSchedule()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var players = CreatePlayers(context, 7);

        var match = new Match
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            Opposition = "Curzon Ashton",
            HalfDurationMinutes = 25
        };
        context.Matches.Add(match);
        await context.SaveChangesAsync();

        var service = new MatchSquadService(context);

        // Act
        var squad = await service.GenerateSquadAsync(new GenerateMatchSquadRequest
        {
            MatchId = match.Id,
            CustomPlayerIds = players.Select(p => p.Id).ToList()
        });

        // Assert
        Assert.Equal(25, squad.HalfDurationMinutes);
        Assert.Equal(6, squad.PeriodDurationMinutes);
        Assert.Equal(8, squad.TotalPeriods); // 4 periods per half (6m, 6m, 6m, 7m)
        Assert.Equal(25, squad.Periods[3].EndMinute); // Period 4: 18' - 25' (7 mins)
        Assert.Equal(7, squad.Periods[3].EndMinute - squad.Periods[3].StartMinute);
        Assert.Equal(50, squad.Periods[7].EndMinute); // Period 8: 43' - 50' (7 mins)
        Assert.Equal(7, squad.Periods[7].EndMinute - squad.Periods[7].StartMinute);
        Assert.All(squad.PlayerMinutes, pm => Assert.InRange(pm.TotalMinutes, 30, 40));
    }

    [Fact]
    public async Task GenerateSquad_With3v3Format_Produces3Outfield_0GK_AndEqualMinutes()
    {
        // Arrange (3v3 format with 4 players: 3 on pitch, 1 on bench, 1 sub per period, 0 GK)
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var players = CreatePlayers(context, 4);

        var match = new Match
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow,
            Opposition = "3v3 Stalybridge",
            HalfDurationMinutes = 18,
            Format = "3v3"
        };
        context.Matches.Add(match);
        await context.SaveChangesAsync();

        var service = new MatchSquadService(context);

        // Act
        var squad = await service.GenerateSquadAsync(new GenerateMatchSquadRequest
        {
            MatchId = match.Id,
            CustomPlayerIds = players.Select(p => p.Id).ToList()
        });

        // Assert
        Assert.Equal("3v3", squad.Format);
        Assert.Null(squad.FirstHalfGoalkeeperPlayerId);
        Assert.Null(squad.SecondHalfGoalkeeperPlayerId);
        Assert.Equal(6, squad.Periods.Count);

        // Every period has 0 GK, 3 Outfield, 1 Bench
        foreach (var period in squad.Periods)
        {
            Assert.Null(period.Goalkeeper);
            Assert.Equal(3, period.OutfieldPlayers.Count);
            Assert.Single(period.BenchPlayers);
        }

        // Each period after the first should have 1 sub
        for (int i = 1; i < squad.Periods.Count; i++)
        {
            Assert.Single(squad.Periods[i].Substitutions);
        }

        // 4 players x 27 mins avg = 108 mins total (3 on pitch x 36 mins total = 108 mins)
        // In discrete 6-min slots, 2 players get 30 mins (5 periods) and 2 players get 24 mins (4 periods)
        Assert.All(squad.PlayerMinutes, pm =>
        {
            Assert.InRange(pm.TotalMinutes, 24, 30);
            Assert.Equal(0, pm.GoalkeeperMinutes);
            Assert.Equal(pm.TotalMinutes, pm.OutfieldMinutes);
            Assert.Equal(36 - pm.TotalMinutes, pm.BenchMinutes);
        });
    }

    [Fact]
    public async Task GenerateSquad_With3v3Format_And5Players_RotatesPlayersFairly()
    {
        // Arrange (3v3 format with 5 players: 3 on pitch, 2 on bench, 2 subs per period)
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var players = CreatePlayers(context, 5);
        await context.SaveChangesAsync();

        var service = new MatchSquadService(context);

        // Act
        var squad = await service.GenerateSquadAsync(new GenerateMatchSquadRequest
        {
            CustomPlayerIds = players.Select(p => p.Id).ToList(),
            Format = "3v3",
            HalfDurationMinutes = 18
        });

        // Assert
        Assert.Equal("3v3", squad.Format);
        Assert.Null(squad.FirstHalfGoalkeeperPlayerId);
        Assert.Null(squad.SecondHalfGoalkeeperPlayerId);

        foreach (var period in squad.Periods)
        {
            Assert.Null(period.Goalkeeper);
            Assert.Equal(3, period.OutfieldPlayers.Count);
            Assert.Equal(2, period.BenchPlayers.Count);
        }

        for (int i = 1; i < squad.Periods.Count; i++)
        {
            Assert.Equal(2, squad.Periods[i].Substitutions.Count);
        }

        // 5 players, 108 total pitch minutes -> avg 21.6 mins (21 or 22 mins / either 18 or 24 min multiples of 6m)
        Assert.All(squad.PlayerMinutes, pm =>
        {
            Assert.InRange(pm.TotalMinutes, 18, 24);
            Assert.Equal(0, pm.GoalkeeperMinutes);
        });
    }
}
