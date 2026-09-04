using System;
using System.Collections.Generic;

namespace Celtic.Api.Models;

public class Team
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. "Stripes", "Hoops"
    public string? ColorHex { get; set; } = "#006837";
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Player> Players { get; set; } = new List<Player>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
