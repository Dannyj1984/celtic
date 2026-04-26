namespace Celtic.Api.Models;

public class Match
{
    public Guid Id { get; set; }
    public Guid SeasonId { get; set; }
    public Season Season { get; set; } = null!;

    public DateTime Date { get; set; }
    public string Opposition { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public string? MatchReport { get; set; }
    public bool IsPublished { get; set; }

    // Computed
    public string Result => GoalsFor > GoalsAgainst ? "Win"
        : GoalsFor < GoalsAgainst ? "Loss"
        : "Draw";

    // Navigation — link to Event (optional, a match can exist without a calendar event)
    public Guid? EventId { get; set; }
    public Event? Event { get; set; }

    public ICollection<MatchAppearance> Appearances { get; set; } = new List<MatchAppearance>();
}
