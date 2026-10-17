using Jrpg.Core.Items;

namespace Jrpg.Core.Tests.Builders;

internal sealed class ItemDefinitionBuilder
{
    private string _id = "potion";
    private int _maxStack = 99;
    private EquipmentStats? _equipment;

    public ItemDefinitionBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public ItemDefinitionBuilder WithMaxStack(int maxStack)
    {
        _maxStack = maxStack;
        return this;
    }

    public ItemDefinitionBuilder AsWeapon(int attackBonus, params string[] allowedClassIds)
    {
        _equipment = new EquipmentStats(EquipmentSlot.Weapon, attackBonus, DefenseBonus: 0, allowedClassIds);
        return this;
    }

    public ItemDefinitionBuilder AsArmor(int defenseBonus, params string[] allowedClassIds)
    {
        _equipment = new EquipmentStats(EquipmentSlot.Armor, AttackBonus: 0, defenseBonus, allowedClassIds);
        return this;
    }

    /// <remarks>Call after <see cref="AsWeapon"/> or <see cref="AsArmor"/>.</remarks>
    public ItemDefinitionBuilder WithBonuses(int attack, int defense)
    {
        _equipment = _equipment! with { AttackBonus = attack, DefenseBonus = defense };
        return this;
    }

    public ItemDefinition Build() => new()
    {
        Id = _id,
        Name = "Test Item",
        Description = "An item for tests.",
        IconFile = "icon.png",
        MaxStack = _maxStack,
        IsUsableFromMenu = false,
        Equipment = _equipment,
    };
}
