using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Celtic.Api.DTOs;
using Celtic.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Celtic.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        await _notificationService.SubscribeAsync(userId, request.Endpoint, request.P256dh, request.Auth);
        return Ok();
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        await _notificationService.UnsubscribeAsync(request.Endpoint, userId);
        return Ok();
    }

    [HttpPost("send-test")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendTestNotification([FromBody] TestNotificationRequest request)
    {
        if (request.ToAll)
        {
            await _notificationService.SendToAllAsync(request.Title, request.Message, request.Url);
        }
        else if (!string.IsNullOrEmpty(request.UserId))
        {
            await _notificationService.SendNotificationAsync(request.UserId, request.Title, request.Message, request.Url);
        }
        else
        {
            return BadRequest("Must specify ToAll or UserId");
        }

        return Ok();
    }
}

public record PushSubscriptionRequest(string Endpoint, string P256dh, string Auth);
public record UnsubscribeRequest(string Endpoint);
public record TestNotificationRequest(string Title, string Message, string? Url, bool ToAll = false, string? UserId = null);
