using System.ComponentModel.DataAnnotations;

namespace Celtic.Api.DTOs;

public record SeasonDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    decimal SubAmount,
    string SubFrequency,
    bool IsCurrent
);

public record CreateSeasonRequest(
    [Required] string Name,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    [Range(0, 1000)] decimal SubAmount,
    [Required] string SubFrequency,
    bool IsCurrent = false
);

public record UpdateSeasonRequest(
    [Required] string Name,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    [Range(0, 1000)] decimal SubAmount,
    [Required] string SubFrequency,
    bool IsCurrent = false
);
