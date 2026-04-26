namespace Celtic.Api.Models;

public class Player
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? MedicalNotes { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<PlayerParent> ParentLinks { get; set; } = new List<PlayerParent>();
    public ICollection<EventResponse> EventResponses { get; set; } = new List<EventResponse>();
    public ICollection<MatchAppearance> MatchAppearances { get; set; } = new List<MatchAppearance>();
    public ICollection<SubPayment> Payments { get; set; } = new List<SubPayment>();

    // Computed
    public string FullName => $"{FirstName} {LastName}";
}
