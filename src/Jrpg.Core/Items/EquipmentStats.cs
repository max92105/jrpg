namespace Jrpg.Core.Items;

/// <summary>What wearing an item gives. Anyone can wear any equipment (decision 165).</summary>
public sealed record EquipmentStats(EquipmentSlot Slot, int AttackBonus, int DefenseBonus);
