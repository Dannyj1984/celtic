namespace Celtic.Api.Models;

public class EventResponse
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;

    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public string Status { get; set; } = "Attending"; // Attending, NotAttending, Maybe
    public string RespondedByUserId { get; set; } = string.Empty;
    public ApplicationUser RespondedBy { get; set; } = null!;
    public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
}
