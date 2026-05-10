using System.Threading.Tasks;
using Celtic.Api.Models;

namespace Celtic.Api.Services;

public interface INotificationService
{
    Task SubscribeAsync(string userId, string endpoint, string p256dh, string auth);
    Task UnsubscribeAsync(string endpoint, string userId);
    Task SendNotificationAsync(string userId, string title, string message, string? url = null);
    Task SendToAllAsync(string title, string message, string? url = null);
}
