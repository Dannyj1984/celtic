using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Celtic.Api.DTOs;
using Celtic.Api.Services;

namespace Celtic.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<FinancialSummaryDto>> GetSummary([FromQuery] Guid seasonId)
    {
        try
        {
            var summary = await _paymentService.GetFinancialSummaryAsync(seasonId);
            return Ok(summary);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("subs")]
    public async Task<ActionResult<List<PlayerSubStatusDto>>> GetSubStatuses(
        [FromQuery] Guid seasonId,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        try
        {
            var statuses = await _paymentService.GetPlayerSubStatusesAsync(seasonId, year, month);
            return Ok(statuses);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("subs")]
    public async Task<ActionResult<SubPaymentDto>> RecordSubPayment([FromBody] RecordSubPaymentRequest request)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can record payments." });

        try
        {
            var payment = await _paymentService.RecordSubPaymentAsync(request);
            return Ok(payment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("subs/{id}")]
    public async Task<ActionResult> DeleteSubPayment(Guid id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can delete payments." });

        var success = await _paymentService.DeleteSubPaymentAsync(id);
        if (!success) return NotFound();

        return NoContent();
    }

    [HttpGet("expenses")]
    public async Task<ActionResult<List<ExpenseDto>>> GetExpenses([FromQuery] Guid seasonId)
    {
        var expenses = await _paymentService.GetExpensesAsync(seasonId);
        return Ok(expenses);
    }

    [HttpPost("expenses")]
    public async Task<ActionResult<ExpenseDto>> CreateExpense([FromBody] CreateExpenseRequest request)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can create expenses." });

        try
        {
            var expense = await _paymentService.CreateExpenseAsync(request);
            return CreatedAtAction(nameof(GetExpenses), new { seasonId = request.SeasonId }, expense);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("expenses/{id}")]
    public async Task<ActionResult<ExpenseDto>> UpdateExpense(Guid id, [FromBody] UpdateExpenseRequest request)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can update expenses." });

        try
        {
            var expense = await _paymentService.UpdateExpenseAsync(id, request);
            return Ok(expense);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("expenses/{id}")]
    public async Task<ActionResult> DeleteExpense(Guid id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin")
            return StatusCode(403, new { message = "Only administrators can delete expenses." });

        var success = await _paymentService.DeleteExpenseAsync(id);
        if (!success) return NotFound();

        return NoContent();
    }
}
