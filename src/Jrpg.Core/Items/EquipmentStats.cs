using Jrpg.Core.Stats;

namespace Jrpg.Core.Items;

/// <summary>What wearing an item gives. Anyone can wear any equipment (decision 165).</summary>
/// <param name="Kind">What the item is worn as, which says the slots it fits (ADR 0028).</param>
public sealed record EquipmentStats(EquipmentKind Kind)
{
    /// <summary>
    /// A weapon's raw damage, which its wearer's basic attack deals before Power (decision 330); 0 for anything else.
    /// </summary>
    public int Damage { get; init; }

    /// <summary>What the item adds by stat kind (ADR 0026); none when content names none.</summary>
    public StatBonuses Bonuses { get; init; } = StatBonuses.None;

    /// <summary>A weapon's hits' type (ADR 0027); null for anything else.</summary>
    public DamageType? DamageType { get; init; }
}
