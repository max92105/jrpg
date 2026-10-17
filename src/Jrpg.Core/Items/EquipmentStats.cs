namespace Jrpg.Core.Items;

/// <summary>What wearing an item gives, and which classes may wear it. An empty class list means nobody can.</summary>
public sealed record EquipmentStats(EquipmentSlot Slot, int AttackBonus, int DefenseBonus, IReadOnlyList<string> AllowedClassIds)
{
    public bool CanBeWornBy(string classId) => AllowedClassIds.Contains(classId);
}
