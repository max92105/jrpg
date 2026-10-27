using Jrpg.Core.Stats;

namespace Jrpg.Core.Items;

/// <summary>What wearing an item gives. Anyone can wear any equipment (decision 165).</summary>
public sealed record EquipmentStats(EquipmentSlot Slot, int AttackBonus, int DefenseBonus)
{
    /// <summary>What the item adds by stat kind (ADR 0026); none when content names none.</summary>
    public StatBonuses Bonuses { get; init; } = StatBonuses.None;

    /// <summary>A weapon's hits' type (ADR 0027); null for anything else.</summary>
    public DamageType? DamageType { get; init; }
}
