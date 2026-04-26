namespace Celtic.Api.Models;

public class PlayerParent
{
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string Relationship { get; set; } = "Parent"; // Mum, Dad, Guardian, etc.
}
