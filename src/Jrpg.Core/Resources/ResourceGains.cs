namespace Jrpg.Core.Resources;

/// <summary>How much a resource gains on each trigger; a negative gain drains it, 0 does nothing (ADR 0019).</summary>
public sealed record ResourceGains
{
    /// <summary>At the end of each of the holder's own turns.</summary>
    public int PerTurn { get; init; }
}
