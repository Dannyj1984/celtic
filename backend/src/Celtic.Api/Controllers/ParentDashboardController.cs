using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Celtic.Api.Data;
using Celtic.Api.DTOs;
using Celtic.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Celtic.Api.Controllers;

[ApiController]
[Route("api/parent")]
[Authorize]
public class ParentDashboardController : ControllerBase
{
    private readonly CelticDbContext _context;

    public ParentDashboardController(CelticDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("User not found");

        var playerParent = await _context.PlayerParents
            .Include(pp => pp.Player)
            .FirstOrDefaultAsync(pp => pp.UserId == userId);

        if (playerParent == null) return NotFound("No linked player found for this parent");

        var settings = await _context.ClubSettings.FirstOrDefaultAsync();
        
        // Calculate seasonal performance
        var currentSeason = await _context.Seasons.FirstOrDefaultAsync(s => s.IsCurrent)
                            ?? await _context.Seasons.OrderByDescending(s => s.StartDate).FirstOrDefaultAsync();

        // Training: Since Sep 1st of the current or previous year
        var trainingStartYear = DateTime.UtcNow.Month >= 9 ? DateTime.UtcNow.Year : DateTime.UtcNow.Year - 1;
        var trainingStartDate = new DateTime(trainingStartYear, 9, 1, 0, 0, 0, DateTimeKind.Utc);

        var trainingEvents = await _context.Events
            .Where(e => e.Type == "Training" && e.DateTime >= trainingStartDate && e.DateTime <= DateTime.UtcNow && !e.IsCancelled)
            .Select(e => e.Id)
            .ToListAsync();

        var trainingAttended = await _context.EventResponses
            .Where(er => er.PlayerId == playerParent.PlayerId && er.Status == "Attending" && trainingEvents.Contains(er.EventId))
            .CountAsync();

        // Matches: Current season events that have passed
        var matchEvents = await _context.Events
            .Where(e => e.Type == "Match" && e.DateTime <= DateTime.UtcNow && !e.IsCancelled)
            .Where(e => currentSeason != null ? e.SeasonId == currentSeason.Id : true)
            .Select(e => e.Id)
            .ToListAsync();

        var matchesAttended = await _context.EventResponses
            .Where(er => er.PlayerId == playerParent.PlayerId && er.Status == "Attending" && matchEvents.Contains(er.EventId))
            .CountAsync();

        // Next Training Event
        var nextTrainingEvent = await _context.Events
            .Where(e => e.Type == "Training" && e.DateTime > DateTime.UtcNow && !e.IsCancelled)
            .OrderBy(e => e.DateTime)
            .FirstOrDefaultAsync();

        // Next Match Event
        var nextMatchEvent = await _context.Events
            .Include(e => e.Match)
            .Where(e => e.Type == "Match" && e.DateTime > DateTime.UtcNow && !e.IsCancelled)
            .OrderBy(e => e.DateTime)
            .FirstOrDefaultAsync();

        var dto = new DashboardDto
        {
            ParentName = user.FullName ?? user.UserName ?? "Parent",
            PlayerName = playerParent.Player.FullName,
            SubscriptionStatus = playerParent.Player.SubscriptionStatus,
            NextSubPaymentDate = settings?.NextSubPaymentDate,
            CoachWhatsAppNumber = settings?.CoachWhatsAppNumber ?? string.Empty,
            AttendingNextTraining = nextTrainingEvent != null && await _context.EventResponses
                .AnyAsync(er => er.EventId == nextTrainingEvent.Id && er.PlayerId == playerParent.PlayerId && er.Status == "Attending"),
            AttendingNextMatch = nextMatchEvent != null && await _context.EventResponses
                .AnyAsync(er => er.EventId == nextMatchEvent.Id && er.PlayerId == playerParent.PlayerId && er.Status == "Attending"),
            CoachNotes = playerParent.Player.CoachNotes,
            AllowPhotos = playerParent.Player.AllowPhotos
        };

        if (nextMatchEvent != null && nextMatchEvent.Match != null)
        {
            dto.NextMatch = new DashboardMatchDto
            {
                Id = nextMatchEvent.Match.Id,
                Date = nextMatchEvent.Match.Date,
                Opposition = nextMatchEvent.Match.Opposition,
                Location = nextMatchEvent.Match.Location
            };
        }

        if (settings != null)
        {
            dto.TrainingSchedule = new DashboardTrainingDto
            {
                Day = settings.TrainingDay.ToString(),
                StartTime = settings.TrainingStartTime.ToString(@"hh\:mm"),
                EndTime = settings.TrainingEndTime.ToString(@"hh\:mm"),
                Location = settings.TrainingLocation,
                TrainingFocus = settings.TrainingFocus,
                GoodToKnow = settings.GoodToKnow
            };
        }

        dto.Performance = new DashboardPerformanceDto
        {
            Training = new PerformanceStatsDto
            {
                TotalSessions = trainingEvents.Count,
                AttendedSessions = trainingAttended
            },
            Matches = new PerformanceStatsDto
            {
                TotalSessions = matchEvents.Count,
                AttendedSessions = matchesAttended
            }
        };

        // Calculate Training Cards Progress
        var cardRewards = new List<CardRewardRuleDto>();
        if (!string.IsNullOrEmpty(settings?.CardRewardsJson))
        {
            try
            {
                cardRewards = System.Text.Json.JsonSerializer.Deserialize<List<CardRewardRuleDto>>(settings.CardRewardsJson, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<CardRewardRuleDto>();
            }
            catch { }
        }

        var sortedRewards = cardRewards.OrderBy(r => r.CardsRequired).ToList();
        var cardsCount = playerParent.Player.TrainingCardsCount;
        var unlocked = sortedRewards.Where(r => r.CardsRequired <= cardsCount).ToList();
        var next = sortedRewards.FirstOrDefault(r => r.CardsRequired > cardsCount);

        dto.CardsProgress = new PlayerCardProgressDto
        {
            CardsCount = cardsCount,
            NextReward = next,
            CardsUntilNextReward = next != null ? next.CardsRequired - cardsCount : null,
            UnlockedRewards = unlocked
        };

        return Ok(dto);
    }

    [HttpPost("actions/photo-consent")]
    public async Task<ActionResult> UpdatePhotoConsent([FromBody] UpdatePhotoConsentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var playerParent = await _context.PlayerParents
            .Include(pp => pp.Player)
            .FirstOrDefaultAsync(pp => pp.UserId == userId);

        if (playerParent == null) return NotFound("No linked player found for this parent");

        playerParent.Player.AllowPhotos = request.AllowPhotos;
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("actions/training")]
    public async Task<ActionResult> RegisterForTraining()
    {
        return await RegisterForNextEvent("Training");
    }

    [HttpPost("actions/match")]
    public async Task<ActionResult> ConfirmMatchAvailability()
    {
        return await RegisterForNextEvent("Match");
    }

    [HttpGet("upcoming/{type}")]
    public async Task<ActionResult<List<UpcomingEventDto>>> GetUpcomingEvents(string type)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var playerParent = await _context.PlayerParents.FirstOrDefaultAsync(pp => pp.UserId == userId);
        if (playerParent == null) return NotFound("No linked player found for this parent");

        var events = await _context.Events
            .Include(e => e.Match)
            .Where(e => e.Type == type && e.DateTime > DateTime.UtcNow)
            .OrderBy(e => e.DateTime)
            .ToListAsync();

        var eventIds = events.Select(e => e.Id).ToList();
        var responses = await _context.EventResponses
            .Where(er => er.PlayerId == playerParent.PlayerId && eventIds.Contains(er.EventId))
            .ToDictionaryAsync(er => er.EventId, er => er.Status);

        var result = events.Select(e => new UpcomingEventDto
        {
            Id = e.Id,
            Type = e.Type,
            DateTime = e.DateTime,
            Location = e.Location,
            Notes = e.Notes,
            Status = responses.ContainsKey(e.Id) ? responses[e.Id] : "No Response",
            Played = responses.ContainsKey(e.Id) && responses[e.Id] == "Attending",
            Opposition = e.Match?.Opposition
        }).ToList();

        return Ok(result);
    }

    [HttpGet("past/{type}")]
    public async Task<ActionResult<List<UpcomingEventDto>>> GetPastEvents(string type)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var playerParent = await _context.PlayerParents.FirstOrDefaultAsync(pp => pp.UserId == userId);
        if (playerParent == null) return NotFound("No linked player found for this parent");

        var events = await _context.Events
            .Include(e => e.Match)
                .ThenInclude(m => m!.PlayerOfTheMatch)
            .Where(e => e.Type == type && e.DateTime <= DateTime.UtcNow)
            .OrderByDescending(e => e.DateTime)
            .ToListAsync();

        var eventIds = events.Select(e => e.Id).ToList();
        var responses = await _context.EventResponses
            .Where(er => er.PlayerId == playerParent.PlayerId && eventIds.Contains(er.EventId))
            .ToDictionaryAsync(er => er.EventId, er => er.Status);

        var result = events.Select(e => new UpcomingEventDto
        {
            Id = e.Id,
            Type = e.Type,
            DateTime = e.DateTime,
            Location = e.Location,
            Notes = e.Notes,
            Status = responses.ContainsKey(e.Id) ? responses[e.Id] : "No Response",
            Played = responses.ContainsKey(e.Id) && responses[e.Id] == "Attending",
            Opposition = e.Match?.Opposition,
            Score = e.Match != null ? $"{e.Match.GoalsFor} - {e.Match.GoalsAgainst}" : null,
            Result = e.Match != null ? (e.Match.GoalsFor > e.Match.GoalsAgainst ? "Win" : e.Match.GoalsFor < e.Match.GoalsAgainst ? "Loss" : "Draw") : null,
            MatchReport = e.Match?.MatchReport,
            PlayerOfTheMatchName = e.Match?.PlayerOfTheMatch?.FullName
        }).ToList();

        return Ok(result);
    }

    [HttpPost("actions/bulk-register")]
    public async Task<ActionResult> BulkRegister([FromBody] BulkRegisterRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var playerParent = await _context.PlayerParents.FirstOrDefaultAsync(pp => pp.UserId == userId);
        if (playerParent == null) return NotFound("No linked player found for this parent");

        foreach (var selection in request.Selections)
        {
            var existingResponse = await _context.EventResponses
                .FirstOrDefaultAsync(er => er.EventId == selection.EventId && er.PlayerId == playerParent.PlayerId);

            if (existingResponse != null)
            {
                existingResponse.Status = selection.Status;
                existingResponse.RespondedByUserId = userId;
                existingResponse.RespondedAt = DateTime.UtcNow;
            }
            else
            {
                _context.EventResponses.Add(new EventResponse
                {
                    Id = Guid.NewGuid(),
                    EventId = selection.EventId,
                    PlayerId = playerParent.PlayerId,
                    Status = selection.Status,
                    RespondedByUserId = userId,
                    RespondedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    private async Task<ActionResult> RegisterForNextEvent(string eventType)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var playerParent = await _context.PlayerParents.FirstOrDefaultAsync(pp => pp.UserId == userId);
        if (playerParent == null) return NotFound("No linked player found for this parent");

        var nextEvent = await _context.Events
            .Where(e => e.Type == eventType && e.DateTime > DateTime.UtcNow)
            .OrderBy(e => e.DateTime)
            .FirstOrDefaultAsync();

        if (nextEvent == null) return NotFound($"No upcoming {eventType} found.");

        var existingResponse = await _context.EventResponses
            .FirstOrDefaultAsync(er => er.EventId == nextEvent.Id && er.PlayerId == playerParent.PlayerId);

        if (existingResponse != null)
        {
            existingResponse.Status = "Attending";
            existingResponse.RespondedByUserId = userId;
            existingResponse.RespondedAt = DateTime.UtcNow;
        }
        else
        {
            _context.EventResponses.Add(new EventResponse
            {
                Id = Guid.NewGuid(),
                EventId = nextEvent.Id,
                PlayerId = playerParent.PlayerId,
                Status = "Attending",
                RespondedByUserId = userId,
                RespondedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("profile")]
    public async Task<ActionResult<PlayerProfileDto>> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var playerParent = await _context.PlayerParents
            .Include(pp => pp.Player)
            .Include(pp => pp.User)
            .FirstOrDefaultAsync(pp => pp.UserId == userId);

        if (playerParent == null) return NotFound("No linked player found");

        var player = playerParent.Player;
        var currentSeason = await _context.Seasons.FirstOrDefaultAsync(s => s.IsCurrent)
                            ?? await _context.Seasons.OrderByDescending(s => s.StartDate).FirstOrDefaultAsync();

        // 1. Seasonal Match Attendance
        var matchEvents = await _context.Events
            .Where(e => e.Type == "Match" && e.DateTime <= DateTime.UtcNow && !e.IsCancelled)
            .Where(e => currentSeason != null ? e.SeasonId == currentSeason.Id : true)
            .Select(e => e.Id)
            .ToListAsync();

        var matchesAttended = await _context.EventResponses
            .Where(er => er.PlayerId == player.Id && er.Status == "Attending" && matchEvents.Contains(er.EventId))
            .CountAsync();

        // 2. Player of the Match stats
        var potmMatches = await _context.Matches
            .Where(m => m.PlayerOfTheMatchId == player.Id)
            .ToListAsync();

        // 3. Recent Matches (Last 3)
        var recentMatches = await _context.Matches
            .Where(m => m.IsPublished && m.Date <= DateTime.UtcNow)
            .OrderByDescending(m => m.Date)
            .Take(3)
            .Select(m => new ProfileMatchDto
            {
                Id = m.Id,
                Date = m.Date,
                Opposition = m.Opposition,
                Result = m.GoalsFor > m.GoalsAgainst ? "Win" : m.GoalsFor < m.GoalsAgainst ? "Loss" : "Draw",
                Score = $"{m.GoalsFor} - {m.GoalsAgainst}",
                WasPlayerOfTheMatch = m.PlayerOfTheMatchId == player.Id
            })
            .ToListAsync();

        // 4. Badges
        var badges = new List<BadgeDto>();

        // Attendance Badge: Last 5 matches AND last 5 training sessions
        var last5Matches = await _context.Events
            .Where(e => e.Type == "Match" && e.DateTime <= DateTime.UtcNow && !e.IsCancelled)
            .OrderByDescending(e => e.DateTime).Take(5).Select(e => e.Id).ToListAsync();
        
        var last5Training = await _context.Events
            .Where(e => e.Type == "Training" && e.DateTime <= DateTime.UtcNow && !e.IsCancelled)
            .OrderByDescending(e => e.DateTime).Take(5).Select(e => e.Id).ToListAsync();

        bool attendedAllMatches = last5Matches.Count > 0;
        foreach(var mid in last5Matches) {
            if (!await _context.EventResponses.AnyAsync(er => er.EventId == mid && er.PlayerId == player.Id && er.Status == "Attending")) {
                attendedAllMatches = false;
                break;
            }
        }

        bool attendedAllTraining = last5Training.Count > 0;
        foreach(var tid in last5Training) {
            if (!await _context.EventResponses.AnyAsync(er => er.EventId == tid && er.PlayerId == player.Id && er.Status == "Attending")) {
                attendedAllTraining = false;
                break;
            }
        }

        if (attendedAllMatches && attendedAllTraining) {
            badges.Add(new BadgeDto { Type = "Attendance", Tier = "Active", Name = "Committed" });
        }

        // PotM Badges
        int potmCount = potmMatches.Count;
        if (potmCount >= 10) badges.Add(new BadgeDto { Type = "PotM", Tier = "Gold", Name = "Match Winner" });
        else if (potmCount >= 5) badges.Add(new BadgeDto { Type = "PotM", Tier = "Silver", Name = "Top Performer" });
        else if (potmCount >= 1) badges.Add(new BadgeDto { Type = "PotM", Tier = "Bronze", Name = "Star Player" });

        var createdYear = playerParent.User != null && playerParent.User.CreatedAt.Year > 2000
            ? playerParent.User.CreatedAt.Year
            : DateTime.UtcNow.Year;

        return Ok(new PlayerProfileDto
        {
            PlayerId = player.Id,
            FullName = player.FullName,
            PreferredFoot = player.PreferredFoot,
            ShirtSize = player.ShirtSize,
            ShortSize = player.ShortSize,
            SockSize = player.SockSize,
            JoinedYear = createdYear,
            CreatedYear = createdYear,
            MatchAttendance = new PerformanceStatsDto
            {
                TotalSessions = matchEvents.Count,
                AttendedSessions = matchesAttended
            },
            PlayerOfTheMatchCount = potmCount,
            Badges = badges,
            RecentMatches = recentMatches
        });
    }

    [HttpPut("preferred-foot")]
    public async Task<IActionResult> UpdatePreferredFoot([FromBody] UpdatePreferredFootDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var playerParent = await _context.PlayerParents
            .Include(pp => pp.Player)
            .FirstOrDefaultAsync(pp => pp.UserId == userId);

        if (playerParent == null) return NotFound("No linked player found");

        if (string.IsNullOrWhiteSpace(request.PreferredFoot))
        {
            return BadRequest("Preferred foot is required.");
        }

        playerParent.Player.PreferredFoot = request.PreferredFoot;
        await _context.SaveChangesAsync();

        return Ok(new { preferredFoot = playerParent.Player.PreferredFoot });
    }

    [HttpPut("kit-sizing")]
    public async Task<IActionResult> UpdateKitSizing([FromBody] UpdateKitSizingDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var playerParent = await _context.PlayerParents
            .Include(pp => pp.Player)
            .FirstOrDefaultAsync(pp => pp.UserId == userId);

        if (playerParent == null) return NotFound("No linked player found");

        playerParent.Player.ShirtSize = request.ShirtSize;
        playerParent.Player.ShortSize = request.ShortSize;
        playerParent.Player.SockSize = request.SockSize;
        await _context.SaveChangesAsync();

        return Ok(new { 
            shirtSize = playerParent.Player.ShirtSize,
            shortSize = playerParent.Player.ShortSize,
            sockSize = playerParent.Player.SockSize
        });
    }

    [HttpGet("account")]
    public async Task<ActionResult<ParentAccountDto>> GetAccount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("User not found");

        return Ok(new ParentAccountDto
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone ?? user.PhoneNumber ?? string.Empty
        });
    }

    [HttpPut("account")]
    public async Task<IActionResult> UpdateAccount([FromBody] UpdateParentAccountDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("User not found");

        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest(new { message = "Full name is required." });

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email address is required." });

        var newEmail = request.Email.Trim();
        if (!string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await _context.Users.AnyAsync(u => u.Id != userId && u.Email == newEmail);
            if (existingUser)
            {
                return BadRequest(new { message = "An account with this email address already exists." });
            }

            user.Email = newEmail;
            user.NormalizedEmail = newEmail.ToUpperInvariant();
            user.UserName = newEmail;
            user.NormalizedUserName = newEmail.ToUpperInvariant();
        }

        user.FullName = request.FullName.Trim();
        user.Phone = (request.Phone ?? "").Trim();
        user.PhoneNumber = (request.Phone ?? "").Trim();

        await _context.SaveChangesAsync();

        return Ok(new ParentAccountDto
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone ?? string.Empty
        });
    }
}
