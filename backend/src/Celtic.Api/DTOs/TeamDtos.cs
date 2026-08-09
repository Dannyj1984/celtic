using System;
using System.ComponentModel.DataAnnotations;

namespace Celtic.Api.DTOs;

public record TeamDto(
    Guid Id,
    string Name,
    string? ColorHex,
    bool IsActive,
    int PlayersCount
);

public class CreateTeamRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? ColorHex { get; set; } = "#006837";
}

public class UpdateTeamRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? ColorHex { get; set; } = "#006837";

    public bool IsActive { get; set; } = true;
}
