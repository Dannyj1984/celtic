namespace Celtic.Api.Models;

public class ClubSettings
{
    public Guid Id { get; set; }
    public DateTime NextSubPaymentDate { get; set; }
    public DayOfWeek TrainingDay { get; set; }
    public TimeSpan TrainingStartTime { get; set; }
    public TimeSpan TrainingEndTime { get; set; }
    public string TrainingLocation { get; set; } = string.Empty;
    public string CoachWhatsAppNumber { get; set; } = string.Empty;
    public string? TrainingFocus { get; set; }
    public string? GoodToKnow { get; set; }
}
