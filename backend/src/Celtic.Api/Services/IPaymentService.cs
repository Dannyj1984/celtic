using Celtic.Api.DTOs;

namespace Celtic.Api.Services;

public interface IPaymentService
{
    Task<FinancialSummaryDto> GetFinancialSummaryAsync(Guid seasonId);
    Task<List<PlayerSubStatusDto>> GetPlayerSubStatusesAsync(Guid seasonId, int? year = null, int? month = null);
    Task<SubPaymentDto> RecordSubPaymentAsync(RecordSubPaymentRequest request);
    Task<bool> DeleteSubPaymentAsync(Guid paymentId);
    Task<List<ExpenseDto>> GetExpensesAsync(Guid seasonId);
    Task<ExpenseDto> CreateExpenseAsync(CreateExpenseRequest request);
    Task<ExpenseDto> UpdateExpenseAsync(Guid expenseId, UpdateExpenseRequest request);
    Task<bool> DeleteExpenseAsync(Guid expenseId);
}
