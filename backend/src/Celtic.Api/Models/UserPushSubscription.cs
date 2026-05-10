using System;
using System.ComponentModel.DataAnnotations;

namespace Celtic.Api.Models;

public class UserPushSubscription
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    [Required]
    public string Endpoint { get; set; } = string.Empty;
    
    public string? P256dh { get; set; }
    public string? Auth { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
