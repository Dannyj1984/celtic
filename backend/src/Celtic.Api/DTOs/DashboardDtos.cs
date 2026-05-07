using System;

namespace Celtic.Api.DTOs;

public class DashboardDto
{
    public string ParentName { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public DateTime? NextSubPaymentDate { get; set; }
    
    public DashboardMatchDto? NextMatch { get; set; }
    public DashboardTrainingDto? TrainingSchedule { get; set; }
    public DashboardPerformanceDto? Performance { get; set; }
    public string CoachWhatsAppNumber { get; set; } = string.Empty;
    public bool AttendingNextTraining { get; set; }
    public bool AttendingNextMatch { get; set; }
}

public class DashboardMatchDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Opposition { get; set; } = string.Empty;
    public string? Location { get; set; }
}

public class DashboardTrainingDto
{
    public string Day { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? TrainingFocus { get; set; }
}

public class DashboardPerformanceDto
{
    public PerformanceStatsDto Training { get; set; } = new();
    public PerformanceStatsDto Matches { get; set; } = new();
}

public class PerformanceStatsDto
{
    public int TotalSessions { get; set; }
    public int AttendedSessions { get; set; }
    public double Percentage => TotalSessions == 0 ? 0 : Math.Round((double)AttendedSessions / TotalSessions * 100, 1);
}

public class UpcomingEventDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Status { get; set; } = "No Response"; // Attending, Not Attending, No Response
    public string? Opposition { get; set; } // For matches
    public string? Score { get; set; }
    public string? Result { get; set; }
    public string? MatchReport { get; set; }
    public string? PlayerOfTheMatchName { get; set; }
}

public class BulkRegisterRequest
{
    public List<EventResponseSelection> Selections { get; set; } = new();
}

public class EventResponseSelection
{
    public Guid EventId { get; set; }
    public string Status { get; set; } = "Attending";
}

public class PlayerProfileDto
{
    public Guid PlayerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PreferredFoot { get; set; }
    public PerformanceStatsDto MatchAttendance { get; set; } = new();
    public int PlayerOfTheMatchCount { get; set; }
    public List<BadgeDto> Badges { get; set; } = new();
    public List<ProfileMatchDto> RecentMatches { get; set; } = new();
}

public class BadgeDto
{
    public string Type { get; set; } = string.Empty; // Attendance, PotM
    public string Tier { get; set; } = string.Empty; // Bronze, Silver, Gold, Active
    public string Name { get; set; } = string.Empty;
}

public class ProfileMatchDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Opposition { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string Score { get; set; } = string.Empty;
    public bool WasPlayerOfTheMatch { get; set; }
}
