namespace Celtic.Api.Models;

public class SubPayment
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public Guid SeasonId { get; set; }
    public Season Season { get; set; } = null!;

    public decimal Amount { get; set; }
    public DateTime PaidDate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string Method { get; set; } = "BankTransfer"; // Cash, BankTransfer
    public string? Notes { get; set; }
}
