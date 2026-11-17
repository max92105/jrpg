using Jrpg.Core.Items;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Loot;

/// <summary>
/// One affix the pool may roll on a dropped piece (ADR 0030): the stat it raises, its value range and the kinds of
/// gear it rolls on. A save keeps only an affix's id and value; its stat is read from here on load (ADR 0029).
/// </summary>
public sealed record AffixDefinition
{
    public required string Id { get; init; }

    public required StatKind Stat { get; init; }

    /// <summary>The lowest value a roll gives, in the stat's bonus unit (ADR 0026).</summary>
    public required int Min { get; init; }

    /// <summary>The highest value a roll gives, in the stat's bonus unit.</summary>
    public required int Max { get; init; }

    /// <summary>The gear kinds it rolls on.</summary>
    public required IReadOnlyList<EquipmentKind> Kinds { get; init; }

    /// <summary>Its chance against the other affixes a piece may roll, relative.</summary>
    public required int Weight { get; init; }

    /// <summary>What the affix adds to a piece's price in a shop (ADR 0031, decision 375).</summary>
    public required int GoldValue { get; init; }

    // A record compares a list by reference; an affix rebuilt with the same kinds in order is the same affix, so the
    // Affixes page closed unchanged records no step (as ItemDefinition does its fixed affixes).
    public bool Equals(AffixDefinition? other) =>
        other is not null
        && (Id, Stat, Min, Max, Weight, GoldValue)
            .Equals((other.Id, other.Stat, other.Min, other.Max, other.Weight, other.GoldValue))
        && Kinds.SequenceEqual(other.Kinds);

    public override int GetHashCode() => HashCode.Combine(Id, Kinds.Count);
}
