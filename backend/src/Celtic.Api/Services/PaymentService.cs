using Microsoft.EntityFrameworkCore;
using Celtic.Api.Data;
using Celtic.Api.DTOs;
using Celtic.Api.Models;

namespace Celtic.Api.Services;

public class PaymentService : IPaymentService
{
    private readonly CelticDbContext _context;

    public PaymentService(CelticDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialSummaryDto> GetFinancialSummaryAsync(Guid seasonId)
    {
        var season = await _context.Seasons
            .Include(s => s.Payments)
            .Include(s => s.Expenses)
            .FirstOrDefaultAsync(s => s.Id == seasonId);

        if (season == null)
        {
            throw new KeyNotFoundException($"Season with ID {seasonId} not found.");
        }

        var activePlayers = await _context.Players
            .Where(p => p.IsActive)
            .ToListAsync();

        var totalIncome = season.Payments.Sum(p => p.Amount);
        var totalExpenses = season.Expenses.Sum(e => e.Amount);

        // Determine current month's start and end
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentMonthEnd = currentMonthStart.AddMonths(1).AddTicks(-1);

        var currentMonthPaidCount = season.Payments
            .Where(p => p.PeriodStart <= currentMonthEnd && p.PeriodEnd >= currentMonthStart)
            .Select(p => p.PlayerId)
            .Distinct()
            .Count();

        // Calculate player up-to-date count for current month
        var upToDatePlayersCount = currentMonthPaidCount;

        return new FinancialSummaryDto
        {
            SeasonId = season.Id,
            SeasonName = season.Name,
            SubAmount = season.SubAmount,
            SubFrequency = season.SubFrequency,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetBalance = totalIncome - totalExpenses,
            ActivePlayersCount = activePlayers.Count,
            UpToDatePlayersCount = upToDatePlayersCount,
            CurrentMonthPaidCount = currentMonthPaidCount,
            CurrentMonthTotalPlayers = activePlayers.Count
        };
    }

    public async Task<List<PlayerSubStatusDto>> GetPlayerSubStatusesAsync(Guid seasonId, int? year = null, int? month = null)
    {
        var season = await _context.Seasons.FirstOrDefaultAsync(s => s.Id == seasonId);
        if (season == null)
        {
            throw new KeyNotFoundException($"Season with ID {seasonId} not found.");
        }

        var players = await _context.Players
            .Include(p => p.Team)
            .Where(p => p.IsActive)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();

        var payments = await _context.SubPayments
            .Where(sp => sp.SeasonId == seasonId)
            .ToListAsync();

        // Determine periods to calculate
        var periods = GeneratePeriods(season, year, month);

        var result = new List<PlayerSubStatusDto>();

        foreach (var player in players)
        {
            var playerPayments = payments.Where(p => p.PlayerId == player.Id).ToList();
            var periodStatuses = new List<SubPeriodStatusDto>();

            var totalPaid = playerPayments.Sum(p => p.Amount);
            var totalDue = periods.Count * season.SubAmount;

            foreach (var (pStart, pEnd, pName) in periods)
            {
                var matchingPayment = playerPayments.FirstOrDefault(p =>
                    p.PeriodStart <= pEnd && p.PeriodEnd >= pStart);

                periodStatuses.Add(new SubPeriodStatusDto
                {
                    PeriodName = pName,
                    PeriodStart = pStart,
                    PeriodEnd = pEnd,
                    IsPaid = matchingPayment != null,
                    PaymentId = matchingPayment?.Id,
                    ExpectedAmount = season.SubAmount,
                    PaidAmount = matchingPayment?.Amount,
                    PaidDate = matchingPayment?.PaidDate,
                    Method = matchingPayment?.Method
                });
            }

            // Check if up-to-date up to current period
            var now = DateTime.UtcNow;
            var currentPeriodStatus = periodStatuses.FirstOrDefault(ps => ps.PeriodStart <= now && ps.PeriodEnd >= now);
            var isUpToDate = currentPeriodStatus == null || currentPeriodStatus.IsPaid;

            result.Add(new PlayerSubStatusDto
            {
                PlayerId = player.Id,
                PlayerName = player.FullName,
                IsActive = player.IsActive,
                Periods = periodStatuses,
                TotalPaidThisSeason = totalPaid,
                TotalDueThisSeason = totalDue,
                IsUpToDate = isUpToDate,
                TeamId = player.TeamId,
                TeamName = player.Team?.Name
            });
        }

        return result;
    }

    public async Task<SubPaymentDto> RecordSubPaymentAsync(RecordSubPaymentRequest request)
    {
        var player = await _context.Players.FindAsync(request.PlayerId);
        if (player == null)
            throw new KeyNotFoundException($"Player with ID {request.PlayerId} not found.");

        var season = await _context.Seasons.FindAsync(request.SeasonId);
        if (season == null)
            throw new KeyNotFoundException($"Season with ID {request.SeasonId} not found.");

        // Check if a payment for this period already exists
        var existing = await _context.SubPayments.FirstOrDefaultAsync(sp =>
            sp.PlayerId == request.PlayerId &&
            sp.SeasonId == request.SeasonId &&
            sp.PeriodStart <= request.PeriodEnd &&
            sp.PeriodEnd >= request.PeriodStart);

        if (existing != null)
        {
            // Update existing payment record
            existing.Amount = request.Amount;
            existing.PaidDate = DateTime.SpecifyKind(request.PaidDate, DateTimeKind.Utc);
            existing.PeriodStart = DateTime.SpecifyKind(request.PeriodStart, DateTimeKind.Utc);
            existing.PeriodEnd = DateTime.SpecifyKind(request.PeriodEnd, DateTimeKind.Utc);
            existing.Method = request.Method;
            existing.Notes = request.Notes;

            await _context.SaveChangesAsync();
            return MapToSubPaymentDto(existing, player.FullName);
        }

        var payment = new SubPayment
        {
            Id = Guid.NewGuid(),
            PlayerId = request.PlayerId,
            SeasonId = request.SeasonId,
            Amount = request.Amount,
            PaidDate = DateTime.SpecifyKind(request.PaidDate, DateTimeKind.Utc),
            PeriodStart = DateTime.SpecifyKind(request.PeriodStart, DateTimeKind.Utc),
            PeriodEnd = DateTime.SpecifyKind(request.PeriodEnd, DateTimeKind.Utc),
            Method = request.Method,
            Notes = request.Notes
        };

        _context.SubPayments.Add(payment);
        await _context.SaveChangesAsync();

        return MapToSubPaymentDto(payment, player.FullName);
    }

    public async Task<bool> DeleteSubPaymentAsync(Guid paymentId)
    {
        var payment = await _context.SubPayments.FindAsync(paymentId);
        if (payment == null) return false;

        _context.SubPayments.Remove(payment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ExpenseDto>> GetExpensesAsync(Guid seasonId)
    {
        var expenses = await _context.Expenses
            .Where(e => e.SeasonId == seasonId)
            .OrderByDescending(e => e.Date)
            .ToListAsync();

        return expenses.Select(MapToExpenseDto).ToList();
    }

    public async Task<ExpenseDto> CreateExpenseAsync(CreateExpenseRequest request)
    {
        var season = await _context.Seasons.FindAsync(request.SeasonId);
        if (season == null)
            throw new KeyNotFoundException($"Season with ID {request.SeasonId} not found.");

        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            SeasonId = request.SeasonId,
            Category = request.Category,
            Description = request.Description,
            Amount = request.Amount,
            Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc),
            PaidBy = request.PaidBy,
            Notes = request.Notes
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        return MapToExpenseDto(expense);
    }

    public async Task<ExpenseDto> UpdateExpenseAsync(Guid expenseId, UpdateExpenseRequest request)
    {
        var expense = await _context.Expenses.FindAsync(expenseId);
        if (expense == null)
            throw new KeyNotFoundException($"Expense with ID {expenseId} not found.");

        expense.Category = request.Category;
        expense.Description = request.Description;
        expense.Amount = request.Amount;
        expense.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
        expense.PaidBy = request.PaidBy;
        expense.Notes = request.Notes;

        await _context.SaveChangesAsync();
        return MapToExpenseDto(expense);
    }

    public async Task<bool> DeleteExpenseAsync(Guid expenseId)
    {
        var expense = await _context.Expenses.FindAsync(expenseId);
        if (expense == null) return false;

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
        return true;
    }

    private static SubPaymentDto MapToSubPaymentDto(SubPayment p, string playerName)
    {
        return new SubPaymentDto(
            p.Id,
            p.PlayerId,
            playerName,
            p.SeasonId,
            p.Amount,
            p.PaidDate,
            p.PeriodStart,
            p.PeriodEnd,
            p.Method,
            p.Notes
        );
    }

    private static ExpenseDto MapToExpenseDto(Expense e)
    {
        return new ExpenseDto(
            e.Id,
            e.SeasonId,
            e.Category,
            e.Description,
            e.Amount,
            e.Date,
            e.PaidBy,
            e.Notes
        );
    }

    private static List<(DateTime Start, DateTime End, string Name)> GeneratePeriods(Season season, int? targetYear, int? targetMonth)
    {
        var periods = new List<(DateTime Start, DateTime End, string Name)>();

        if (targetYear.HasValue && targetMonth.HasValue)
        {
            var start = new DateTime(targetYear.Value, targetMonth.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1).AddTicks(-1);
            periods.Add((start, end, start.ToString("MMMM yyyy")));
            return periods;
        }

        // Generate monthly periods from season start to season end
        var current = new DateTime(season.StartDate.Year, season.StartDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = season.EndDate.ToUniversalTime();

        while (current <= endDate)
        {
            var periodEnd = current.AddMonths(1).AddTicks(-1);
            periods.Add((current, periodEnd, current.ToString("MMMM yyyy")));
            current = current.AddMonths(1);
        }

        if (periods.Count == 0)
        {
            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1).AddTicks(-1);
            periods.Add((start, end, start.ToString("MMMM yyyy")));
        }

        return periods;
    }
}
