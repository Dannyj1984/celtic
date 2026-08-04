using System.ComponentModel.DataAnnotations;

namespace Celtic.Api.DTOs;

public record PlayerDto(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string? MedicalNotes,
    string? EmergencyContact,
    string? EmergencyPhone,
    string? EmergencyContact2,
    string? EmergencyPhone2,
    bool IsActive,
    string SubscriptionStatus,
    List<PlayerParentDto> Parents,
    PlayerAttendanceDto? Attendance = null,
    string PreferredFoot = "Right",
    string? CoachNotes = null,
    string? FanNumber = null,
    string? ShirtSize = null,
    string? Allergies = null
);

public record PlayerAttendanceDto(
    int TrainingAttended,
    int TrainingTotal,
    int MatchAttended,
    int MatchTotal
);

public record PlayerParentDto(
    string UserId,
    string FullName,
    string Email,
    string? Phone,
    string Relationship
);

public record UpdateSubscriptionStatusRequest(
    [Required] string SubscriptionStatus
);

public record CreatePlayerRequest(
    [Required] string FirstName,
    [Required] string LastName,
    DateTime? DateOfBirth,
    string? MedicalNotes,
    string? EmergencyContact,
    string? EmergencyPhone,
    string? EmergencyContact2,
    string? EmergencyPhone2,
    string PreferredFoot = "Right",
    string? CoachNotes = null,
    string? FanNumber = null,
    string? ShirtSize = null,
    string? Allergies = null
);

public record UpdatePlayerRequest(
    [Required] string FirstName,
    [Required] string LastName,
    DateTime? DateOfBirth,
    string? MedicalNotes,
    string? EmergencyContact,
    string? EmergencyPhone,
    string? EmergencyContact2,
    string? EmergencyPhone2,
    bool IsActive,
    string SubscriptionStatus,
    string PreferredFoot = "Right",
    string? CoachNotes = null,
    string? FanNumber = null,
    string? ShirtSize = null,
    string? Allergies = null
);
