namespace Celtic.Api.Models;

public class Season
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. "2026-27"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal SubAmount { get; set; } // Per period
    public string SubFrequency { get; set; } = "Monthly"; // Weekly, Monthly, Termly
    public bool IsCurrent { get; set; }

    // Navigation
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
    public ICollection<SubPayment> Payments { get; set; } = new List<SubPayment>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
