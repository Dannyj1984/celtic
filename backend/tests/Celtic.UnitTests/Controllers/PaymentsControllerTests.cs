using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Celtic.Api.Controllers;
using Celtic.Api.DTOs;
using Celtic.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Celtic.UnitTests.Controllers;

public class PaymentsControllerTests
{
    private PaymentsController CreateController(IPaymentService service, string role = "Admin")
    {
        var controller = new PaymentsController(service);
        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        }, "mock"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = userClaims }
        };

        return controller;
    }

    [Fact]
    public async Task GetSummary_ReturnsOkResultWithSummary()
    {
        // Arrange
        var mockService = new Mock<IPaymentService>();
        var seasonId = Guid.NewGuid();
        var summaryDto = new FinancialSummaryDto
        {
            SeasonId = seasonId,
            SeasonName = "2026-27",
            TotalIncome = 500m,
            TotalExpenses = 200m,
            NetBalance = 300m
        };

        mockService.Setup(s => s.GetFinancialSummaryAsync(seasonId))
            .ReturnsAsync(summaryDto);

        var controller = CreateController(mockService.Object);

        // Act
        var result = await controller.GetSummary(seasonId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<FinancialSummaryDto>(okResult.Value);
        Assert.Equal(500m, returned.TotalIncome);
        Assert.Equal(300m, returned.NetBalance);
    }

    [Fact]
    public async Task RecordSubPayment_NonAdmin_Returns403()
    {
        // Arrange
        var mockService = new Mock<IPaymentService>();
        var controller = CreateController(mockService.Object, role: "User");

        var request = new RecordSubPaymentRequest
        {
            PlayerId = Guid.NewGuid(),
            SeasonId = Guid.NewGuid(),
            Amount = 30m,
            PaidDate = DateTime.UtcNow,
            PeriodStart = DateTime.UtcNow,
            PeriodEnd = DateTime.UtcNow
        };

        // Act
        var result = await controller.RecordSubPayment(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task CreateExpense_Admin_ReturnsCreated()
    {
        // Arrange
        var mockService = new Mock<IPaymentService>();
        var seasonId = Guid.NewGuid();
        var expenseDto = new ExpenseDto(
            Guid.NewGuid(),
            seasonId,
            "PitchHire",
            "Match pitch rental",
            100m,
            DateTime.UtcNow,
            "Admin",
            null
        );

        mockService.Setup(s => s.CreateExpenseAsync(It.IsAny<CreateExpenseRequest>()))
            .ReturnsAsync(expenseDto);

        var controller = CreateController(mockService.Object, role: "Admin");

        var request = new CreateExpenseRequest
        {
            SeasonId = seasonId,
            Category = "PitchHire",
            Description = "Match pitch rental",
            Amount = 100m,
            Date = DateTime.UtcNow
        };

        // Act
        var result = await controller.CreateExpense(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returned = Assert.IsType<ExpenseDto>(createdResult.Value);
        Assert.Equal(100m, returned.Amount);
    }
}
