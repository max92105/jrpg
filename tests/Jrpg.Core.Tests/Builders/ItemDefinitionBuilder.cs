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

    public ItemDefinitionBuilder AsWeapon(int damage)
    {
        _equipment = new EquipmentStats(EquipmentSlot.Weapon) { Damage = damage };
        return this;
    }

    public ItemDefinitionBuilder AsArmor()
    {
        _equipment = new EquipmentStats(EquipmentSlot.Armor);
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
