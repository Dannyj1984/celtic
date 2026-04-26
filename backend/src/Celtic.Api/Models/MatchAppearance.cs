namespace Celtic.Api.Models;

public class MatchAppearance
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public int GoalsScored { get; set; }
    public bool PlayerOfMatch { get; set; }
}
