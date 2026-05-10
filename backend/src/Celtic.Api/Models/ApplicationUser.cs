using Microsoft.AspNetCore.Identity;

namespace Celtic.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = "Parent"; // "Admin" or "Parent"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<PlayerParent> PlayerLinks { get; set; } = new List<PlayerParent>();
    public ICollection<UserPushSubscription> PushSubscriptions { get; set; } = new List<UserPushSubscription>();
}
