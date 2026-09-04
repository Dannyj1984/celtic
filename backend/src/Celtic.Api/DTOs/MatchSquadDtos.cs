using System.ComponentModel.DataAnnotations;

namespace Celtic.Api.DTOs;

public record SquadPlayerDto(
    Guid Id,
    string Name,
    string? Position = null
);

public record SubstitutionInfoDto(
    Guid PlayerInId,
    string PlayerInName,
    Guid PlayerOutId,
    string PlayerOutName
);

public record SquadPeriodDto(
    int PeriodNumber,
    int Half,
    int StartMinute,
    int EndMinute,
    SquadPlayerDto? Goalkeeper,
    List<SquadPlayerDto> OutfieldPlayers,
    List<SquadPlayerDto> BenchPlayers,
    List<SubstitutionInfoDto> Substitutions
);

public record PlayerMinutesDto(
    Guid PlayerId,
    string PlayerName,
    int TotalMinutes,
    int GoalkeeperMinutes,
    int OutfieldMinutes,
    int BenchMinutes
);

public record MatchSquadDto(
    Guid Id,
    Guid? MatchId,
    Guid? EventId,
    int HalfDurationMinutes,
    string Format,
    int TotalPeriods,
    int PeriodDurationMinutes,
    Guid? FirstHalfGoalkeeperPlayerId,
    string? FirstHalfGoalkeeperName,
    Guid? SecondHalfGoalkeeperPlayerId,
    string? SecondHalfGoalkeeperName,
    List<SquadPlayerDto> RegisteredPlayers,
    List<SquadPeriodDto> Periods,
    List<PlayerMinutesDto> PlayerMinutes,
    DateTime UpdatedAt
);

public record GenerateMatchSquadRequest(
    Guid? MatchId = null,
    Guid? EventId = null,
    Guid? FirstHalfGoalkeeperPlayerId = null,
    Guid? SecondHalfGoalkeeperPlayerId = null,
    int? HalfDurationMinutes = null,
    string? Format = null,
    int? TotalPeriods = null,
    int? PeriodDurationMinutes = null,
    List<Guid>? CustomPlayerIds = null
);

public record SaveMatchSquadRequest(
    Guid? MatchId = null,
    Guid? EventId = null,
    int HalfDurationMinutes = 18,
    string Format = "5v5",
    int TotalPeriods = 6,
    int PeriodDurationMinutes = 6,
    Guid? FirstHalfGoalkeeperPlayerId = null,
    Guid? SecondHalfGoalkeeperPlayerId = null,
    [Required] List<SquadPeriodDto> Periods = null!,
    List<SquadPlayerDto>? RegisteredPlayers = null
);
