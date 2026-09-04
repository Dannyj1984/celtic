using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Celtic.Api.DTOs;

public record SubPaymentDto(
    Guid Id,
    Guid PlayerId,
    string PlayerName,
    Guid SeasonId,
    decimal Amount,
    DateTime PaidDate,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    string Method,
    string? Notes
);

public record ExpenseDto(
    Guid Id,
    Guid SeasonId,
    string Category,
    string Description,
    decimal Amount,
    DateTime Date,
    string? PaidBy,
    string? Notes
);

public class RecordSubPaymentRequest
{
    [Required]
    public Guid PlayerId { get; set; }

    [Required]
    public Guid SeasonId { get; set; }

    [Range(0, 100000)]
    public decimal Amount { get; set; }

    [Required]
    public DateTime PaidDate { get; set; }

    [Required]
    public DateTime PeriodStart { get; set; }

    [Required]
    public DateTime PeriodEnd { get; set; }

    public string Method { get; set; } = "BankTransfer"; // BankTransfer, Cash, StandingOrder, Other
    public string? Notes { get; set; }
}

public class CreateExpenseRequest
{
    [Required]
    public Guid SeasonId { get; set; }

    [Required]
    public string Category { get; set; } = "PitchHire"; // PitchHire, Kit, Equipment, Referee, Tournament, Other

    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(0, 100000)]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public string? PaidBy { get; set; }
    public string? Notes { get; set; }
}

public class UpdateExpenseRequest
{
    [Required]
    public string Category { get; set; } = "PitchHire";

    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(0, 100000)]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public string? PaidBy { get; set; }
    public string? Notes { get; set; }
}

public class SubPeriodStatusDto
{
    public string PeriodName { get; set; } = string.Empty; // e.g. "August 2026"
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public bool IsPaid { get; set; }
    public Guid? PaymentId { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal? PaidAmount { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? Method { get; set; }
}

public class PlayerSubStatusDto
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<SubPeriodStatusDto> Periods { get; set; } = new();
    public decimal TotalPaidThisSeason { get; set; }
    public decimal TotalDueThisSeason { get; set; }
    public bool IsUpToDate { get; set; }
    public Guid? TeamId { get; set; }
    public string? TeamName { get; set; }
}

public class FinancialSummaryDto
{
    public Guid SeasonId { get; set; }
    public string SeasonName { get; set; } = string.Empty;
    public decimal SubAmount { get; set; }
    public string SubFrequency { get; set; } = "Monthly";
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetBalance { get; set; }
    public int ActivePlayersCount { get; set; }
    public int UpToDatePlayersCount { get; set; }
    public int CurrentMonthPaidCount { get; set; }
    public int CurrentMonthTotalPlayers { get; set; }
}
