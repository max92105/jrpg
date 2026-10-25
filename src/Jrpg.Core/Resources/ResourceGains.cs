namespace Jrpg.Core.Resources;

/// <summary>How much a resource gains on each trigger; a negative gain drains it, 0 does nothing (ADR 0019).</summary>
public sealed record ResourceGains
{
    /// <summary>At the end of each of the holder's own turns.</summary>
    public int PerTurn { get; init; }

    /// <summary>Whenever another combatant on the holder's side falls, while the holder still stands.</summary>
    public int PerAllyFallen { get; init; }

    /// <summary>For each hit that costs the holder HP, whatever the damage (question 208).</summary>
    public int PerHitTaken { get; init; }
}
