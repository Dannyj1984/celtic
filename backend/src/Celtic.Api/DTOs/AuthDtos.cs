using System.ComponentModel.DataAnnotations;

namespace Celtic.Api.DTOs;

// === Auth DTOs ===

public record LoginRequest(
    [Required] string Email,
    [Required] string Password
);

public record LoginResponse(
    string Token,
    string UserId,
    string Email,
    string FullName,
    string Role
);

public record CreateAccountRequest(
    [Required] string Email,
    [Required] string FullName,
    [Required] string Password,
    string? Phone,
    string Role = "Parent" // Admin or Parent
);

public record CreateAccountResponse(
    string UserId,
    string Email,
    string FullName,
    string Role
);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required] string NewPassword
);

public record UserInfoResponse(
    string UserId,
    string Email,
    string FullName,
    string Phone,
    string Role,
    List<LinkedPlayerDto> Children
);

public record LinkedPlayerDto(
    Guid PlayerId,
    string FirstName,
    string LastName,
    string Relationship,
    string SubscriptionStatus
);

public record LinkPlayerRequest(
    [Required] Guid PlayerId,
    [Required] string UserId,
    [Required] string Relationship
);
