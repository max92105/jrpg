using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Builders;

internal sealed class ItemDefinitionBuilder
{
    private string _id = "potion";
    private int _maxStack = 99;
    private EquipmentStats? _equipment;
    private Effect? _effect;

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

    public ItemDefinitionBuilder AsWeapon(int attackBonus)
    {
        _equipment = new EquipmentStats(EquipmentSlot.Weapon, attackBonus, DefenseBonus: 0);
        return this;
    }

    public ItemDefinitionBuilder AsArmor(int defenseBonus)
    {
        _equipment = new EquipmentStats(EquipmentSlot.Armor, AttackBonus: 0, defenseBonus);
        return this;
    }

    /// <remarks>Call after <see cref="AsWeapon"/> or <see cref="AsArmor"/>.</remarks>
    public ItemDefinitionBuilder WithAttackAndDefense(int attack, int defense)
    {
        _equipment = _equipment! with { AttackBonus = attack, DefenseBonus = defense };
        return this;
    }

    /// <remarks>Call after <see cref="AsWeapon"/>.</remarks>
    public ItemDefinitionBuilder WithDamageType(DamageType damageType)
    {
        _equipment = _equipment! with { DamageType = damageType };
        return this;
    }

    /// <remarks>Call after <see cref="AsWeapon"/> or <see cref="AsArmor"/>.</remarks>
    public ItemDefinitionBuilder WithBonuses(StatBonuses bonuses)
    {
        _equipment = _equipment! with { Bonuses = bonuses };
        return this;
    }

    public ItemDefinitionBuilder WithEffect(Effect effect)
    {
        _effect = effect;
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
        Effect = _effect,
        Equipment = _equipment,
    };
}
