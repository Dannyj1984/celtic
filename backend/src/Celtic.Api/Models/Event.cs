namespace Celtic.Api.Models;

public class Event
{
    public Guid Id { get; set; }
    public Guid? SeasonId { get; set; }
    public Season? Season { get; set; }

    public string Type { get; set; } = "Training"; // "Training" or "Match"
    public DateTime DateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsCancelled { get; set; }

    // Link to match (only for Match-type events)
    public Guid? MatchId { get; set; }
    public Match? Match { get; set; }

    // Session Captains
    public Guid? Captain1PlayerId { get; set; }
    public Player? Captain1Player { get; set; }
    public Guid? Captain2PlayerId { get; set; }
    public Player? Captain2Player { get; set; }

    // Navigation
    public ICollection<EventResponse> Responses { get; set; } = new List<EventResponse>();
}
