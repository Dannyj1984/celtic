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
    public string? EmergencyContact2 { get; set; }
    public string? EmergencyPhone2 { get; set; }
    public bool IsActive { get; set; } = true;
    public string SubscriptionStatus { get; set; } = "Active";
    public string PreferredFoot { get; set; } = "Right";
    public string? CoachNotes { get; set; }
    public string? FanNumber { get; set; }
    public string? ShirtSize { get; set; }
    public string? ShortSize { get; set; }
    public int? SockSize { get; set; }
    public string? Allergies { get; set; }
    public bool AllowPhotos { get; set; } = false;
    public int TrainingCardsCount { get; set; } = 0;

    // Navigation
    public ICollection<PlayerParent> ParentLinks { get; set; } = new List<PlayerParent>();
    public ICollection<EventResponse> EventResponses { get; set; } = new List<EventResponse>();
    public ICollection<MatchAppearance> MatchAppearances { get; set; } = new List<MatchAppearance>();
    public ICollection<SubPayment> Payments { get; set; } = new List<SubPayment>();

    // Computed
    public string FullName => $"{FirstName} {LastName}";
}
