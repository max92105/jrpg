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
    private bool _isMaterial;

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
        _equipment = new EquipmentStats(EquipmentKind.Weapon) { Damage = damage };
        return this;
    }

    public ItemDefinitionBuilder AsChest() => AsWorn(EquipmentKind.Chest);

    public ItemDefinitionBuilder AsWorn(EquipmentKind kind)
    {
        _equipment = new EquipmentStats(kind);
        return this;
    }

    /// <remarks>Call after <see cref="AsWeapon"/>.</remarks>
    public ItemDefinitionBuilder WithDamageType(DamageType damageType)
    {
        _equipment = _equipment! with { DamageType = damageType };
        return this;
    }

    /// <remarks>Call after <see cref="AsWeapon"/>, <see cref="AsChest"/> or <see cref="AsWorn"/>.</remarks>
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

    public ItemDefinitionBuilder AsMaterial()
    {
        _isMaterial = true;
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
        IsMaterial = _isMaterial,
    };
}
