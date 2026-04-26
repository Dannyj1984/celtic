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
    bool IsActive
);

public record CreatePlayerRequest(
    [Required] string FirstName,
    [Required] string LastName,
    DateTime? DateOfBirth,
    string? MedicalNotes,
    string? EmergencyContact,
    string? EmergencyPhone
);

public record UpdatePlayerRequest(
    [Required] string FirstName,
    [Required] string LastName,
    DateTime? DateOfBirth,
    string? MedicalNotes,
    string? EmergencyContact,
    string? EmergencyPhone,
    bool IsActive
);
