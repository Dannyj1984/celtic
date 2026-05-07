using System.ComponentModel.DataAnnotations;

namespace Celtic.Api.DTOs;

public record EventDto(
    Guid Id,
    Guid? SeasonId,
    string? SeasonName,
    string Type,
    DateTime DateTime,
    string Location,
    string? Notes,
    bool IsCancelled,
    Guid? MatchId,
    List<AttendingPlayerDto> AttendingPlayers
);

public record AttendingPlayerDto(
    Guid PlayerId,
    string FullName
);

public record CreateEventRequest(
    Guid? SeasonId,
    [Required] string Type, // "Training" or "Match"
    [Required] DateTime DateTime,
    [Required] string Location,
    string? Notes
);

public record UpdateEventRequest(
    [Required] DateTime DateTime,
    [Required] string Location,
    string? Notes,
    bool IsCancelled
);
