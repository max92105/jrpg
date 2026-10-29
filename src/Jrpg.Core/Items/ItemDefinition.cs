using Jrpg.Core.Effects;

namespace Jrpg.Core.Items;

/// <summary>One item type. A consumable has an <see cref="Effect"/>; a weapon or armor has <see cref="Equipment"/>.</summary>
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
}
