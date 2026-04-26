namespace Celtic.Api.Models;

public class Expense
{
    public Guid Id { get; set; }
    public Guid SeasonId { get; set; }
    public Season Season { get; set; } = null!;

    public string Category { get; set; } = "PitchHire"; // PitchHire, Kit, Equipment, Other
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? PaidBy { get; set; } // Who paid (coach name etc.)
    public string? Notes { get; set; }
}
