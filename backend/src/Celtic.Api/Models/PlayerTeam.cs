using System;

namespace Celtic.Api.Models;

public class PlayerTeam
{
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
}
