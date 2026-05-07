using System.ComponentModel.DataAnnotations;

namespace Celtic.Api.DTOs;

public record MatchDto(
    Guid Id,
    Guid? SeasonId,
    string? SeasonName,
    DateTime Date,
    string Opposition,
    string? Location,
    int GoalsFor,
    int GoalsAgainst,
    string? MatchReport,
    bool IsPublished,
    string Result,
    Guid? EventId,
    Guid? PlayerOfTheMatchId = null,
    string? PlayerOfTheMatchName = null
);

public record CreateMatchRequest(
    Guid? SeasonId,
    [Required] DateTime Date,
    [Required] string Opposition,
    string? Location,
    string? Notes // For the associated event
);

public record UpdateMatchRequest(
    Guid? SeasonId,
    [Required] DateTime Date,
    [Required] string Opposition,
    string? Location,
    int GoalsFor,
    int GoalsAgainst,
    string? MatchReport,
    bool IsPublished,
    Guid? PlayerOfTheMatchId = null
);
