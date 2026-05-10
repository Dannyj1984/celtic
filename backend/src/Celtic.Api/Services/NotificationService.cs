using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Celtic.Api.Data;
using Celtic.Api.Models;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Celtic.Api.Services;

public class NotificationService : INotificationService
{
    private readonly CelticDbContext _db;
    private readonly ILogger<NotificationService> _logger;
    private readonly PushServiceClient _pushClient;
    private readonly VapidAuthentication? _vapidAuthentication;

    public NotificationService(CelticDbContext db, IConfiguration config, ILogger<NotificationService> logger)
    {
        _db = db;
        _logger = logger;
        
        var publicKey = config["Vapid:PublicKey"];
        var privateKey = config["Vapid:PrivateKey"];
        var subject = config["Vapid:Subject"] ?? "mailto:admin@celtic.app";

        try
        {
            if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(privateKey))
            {
                _logger.LogWarning("VAPID keys are not configured. Push notifications will not work.");
            }
            else
            {
                _vapidAuthentication = new VapidAuthentication(publicKey, privateKey)
                {
                    Subject = subject
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize VAPID authentication. Push notifications will not work.");
        }

        _pushClient = new PushServiceClient();
    }

    public async Task SubscribeAsync(string userId, string endpoint, string p256dh, string auth)
    {
        // Remove existing subscription for this endpoint if any
        var existing = await _db.UserPushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (existing != null)
        {
            existing.UserId = userId;
            existing.P256dh = p256dh;
            existing.Auth = auth;
        }
        else
        {
            _db.UserPushSubscriptions.Add(new UserPushSubscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Endpoint = endpoint,
                P256dh = p256dh,
                Auth = auth
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task UnsubscribeAsync(string endpoint)
    {
        var sub = await _db.UserPushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (sub != null)
        {
            _db.UserPushSubscriptions.Remove(sub);
            await _db.SaveChangesAsync();
        }
    }

    public async Task SendNotificationAsync(string userId, string title, string message, string? url = null)
    {
        var subs = await _db.UserPushSubscriptions.Where(s => s.UserId == userId).ToListAsync();
        foreach (var sub in subs)
        {
            await SendToSubscriptionAsync(sub, title, message, url);
        }
    }

    public async Task SendToAllAsync(string title, string message, string? url = null)
    {
        var subs = await _db.UserPushSubscriptions.ToListAsync();
        foreach (var sub in subs)
        {
            await SendToSubscriptionAsync(sub, title, message, url);
        }
    }

    private async Task SendToSubscriptionAsync(UserPushSubscription sub, string title, string message, string? url)
    {
        if (_vapidAuthentication == null)
        {
            _logger.LogWarning("VAPID authentication is not initialized. Cannot send push notification.");
            return;
        }

        try
        {
            var pushSubscription = new PushSubscription
            {
                Endpoint = sub.Endpoint,
                Keys = new Dictionary<string, string>
                {
                    { "p256dh", sub.P256dh ?? "" },
                    { "auth", sub.Auth ?? "" }
                }
            };

            var payload = JsonSerializer.Serialize(new
            {
                notification = new
                {
                    title,
                    body = message,
                    icon = "/pwa-192x192.png",
                    data = new { url = url ?? "/" }
                }
            });

            var pushMessage = new PushMessage(payload)
            {
                Topic = "general",
                Urgency = PushMessageUrgency.Normal
            };

            await _pushClient.RequestPushMessageDeliveryAsync(pushSubscription, pushMessage, _vapidAuthentication);
        }
        catch (PushServiceClientException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.Gone || ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Subscription expired or gone. Removing from database.");
                _db.UserPushSubscriptions.Remove(sub);
                await _db.SaveChangesAsync();
            }
            else
            {
                _logger.LogError(ex, "Error sending push notification to {Endpoint}", sub.Endpoint);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending push notification to {Endpoint}", sub.Endpoint);
        }
    }
}
