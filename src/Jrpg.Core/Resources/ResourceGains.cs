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

    /// <summary>
    /// For a hit that takes at least <see cref="HeavyHitPercent"/> of the holder's max HP, on top of
    /// <see cref="PerHitTaken"/>.
    /// </summary>
    public int PerHeavyHit { get; init; }

    /// <summary>The share of max HP, 1 to 100, that makes a hit heavy (question 212).</summary>
    public int HeavyHitPercent { get; init; }

    /// <summary>Whenever a combatant on the other side falls, while the holder still stands.</summary>
    public int PerEnemyFallen { get; init; }

    /// <summary>For each of the holder's own hits that costs its target HP, attacks included (decision 206).</summary>
    public int PerHitLanded { get; init; }

    /// <summary>Once each battle ends, whatever the outcome, for a lasting resource (ADR 0024).</summary>
    public int PerBattleFought { get; init; }
}
