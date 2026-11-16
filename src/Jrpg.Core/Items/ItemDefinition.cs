using Jrpg.Core.Effects;

namespace Jrpg.Core.Items;

/// <summary>
/// One item type. A consumable has an <see cref="Effect"/>; a weapon or armor has <see cref="Equipment"/>, and a unique
/// is equipment with fixed affixes (ADR 0058). Two items compare their fixed affixes in order and every other member as
/// a record does (decision 604).
/// </summary>
public sealed record ItemDefinition
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string IconFile { get; init; }

    /// <summary>The most copies the inventory holds; extra copies don't fit.</summary>
    public required int MaxStack { get; init; }

    public required bool IsUsableFromMenu { get; init; }

    public Effect? Effect { get; init; }

    public EquipmentStats? Equipment { get; init; }

    /// <summary>
    /// A crafting material (decision 339): it stacks and is never used or worn. Material is not a <see cref="Rarity"/>.
    /// </summary>
    public bool IsMaterial { get; init; }

    /// <summary>What it costs in a shop (ADR 0031); null is never bought nor sold.</summary>
    public int? Price { get; init; }

    /// <summary>
    /// The strip in the effects folder an item used in battle plays instead of its user's attack strip, paced by its
    /// <c>effect-strips.json</c> record (decision 678); null plays the user's.
    /// </summary>
    public string? EffectFile { get; init; }

    /// <summary>A hand-made piece (ADR 0058): never rolled, its affixes fixed on the record.</summary>
    public bool IsUnique { get; init; }

    /// <summary>
    /// A unique's affixes in order, each value fixed (decision 699), each stat resolved from the pool at load; empty on
    /// every other item.
    /// </summary>
    public IReadOnlyList<RolledAffix> FixedAffixes { get; init; } = [];

    // A record compares a list by reference; an item rebuilt with the same fixed affixes is the same item. The members
    // go through a tuple's Equals, as the record's own equality would compare them: a == per member is a branch each,
    // past the complexity bound (CA1502).
    public bool Equals(ItemDefinition? other) =>
        other is not null
        && (Id, Name, Description, IconFile, MaxStack, IsUsableFromMenu)
            .Equals((other.Id, other.Name, other.Description, other.IconFile, other.MaxStack, other.IsUsableFromMenu))
        && (Effect, Equipment, IsMaterial, Price, EffectFile, IsUnique)
            .Equals((other.Effect, other.Equipment, other.IsMaterial, other.Price, other.EffectFile, other.IsUnique))
        && FixedAffixes.SequenceEqual(other.FixedAffixes);

    public override int GetHashCode() => HashCode.Combine(Id, FixedAffixes.Count);
}
