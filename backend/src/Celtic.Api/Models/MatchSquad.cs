namespace Celtic.Api.Models;

public class MatchSquad
{
    public Guid Id { get; set; }

    public Guid? MatchId { get; set; }
    public Match? Match { get; set; }

    public Guid? EventId { get; set; }
    public Event? Event { get; set; }

    public int HalfDurationMinutes { get; set; } = 18;
    public string Format { get; set; } = "5v5";
    public int TotalPeriods { get; set; } = 6;
    public int PeriodDurationMinutes { get; set; } = 6;

    public Guid? FirstHalfGoalkeeperPlayerId { get; set; }
    public Player? FirstHalfGoalkeeperPlayer { get; set; }

    public Guid? SecondHalfGoalkeeperPlayerId { get; set; }
    public Player? SecondHalfGoalkeeperPlayer { get; set; }

    // Serialized JSON containing detailed period-by-period lineup and substitutions
    public string SquadDataJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
