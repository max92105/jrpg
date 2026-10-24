using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Data.Content;
using Jrpg.Data.Items;

namespace Jrpg.Data.Tests.Items;

public sealed class ItemCatalogValidatorTests
{
    private const string FileName = "items.json";

    private static readonly ItemDefinition _potion = new()
    {
        Id = "potion",
        Name = "Potion",
        Description = "Restores a small amount of HP.",
        IconFile = "Consumable.png",
        MaxStack = 99,
        IsUsableFromMenu = true,
        Effect = new Effect(EffectKind.RestoreHp, Power: 15, Targeting.SingleAlly),
    };

    [Fact]
    public void Validate_ValidItems_DoesNotThrow()
    {
        var exception = Record.Exception(() => ItemCatalogValidator.Validate(FileName, [_potion, _potion with { Id = "hi_potion" }]));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_DuplicateId_ThrowsNamingTheId()
    {
        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [_potion, _potion]));

        Assert.Equal("items.json: item id 'potion' is used more than once.", exception.Message);
    }

    [Fact]
    public void Validate_NullEntry_ThrowsNamingTheIndex()
    {
        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [_potion, null!]));

        Assert.Equal("items.json: entry 1 is null.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmptyName_ThrowsNamingTheField(string name)
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ItemCatalogValidator.Validate(FileName, [_potion with { Name = name }]));

        Assert.Equal("items.json: item 'potion': name must not be empty.", exception.Message);
    }

    [Fact]
    public void Validate_MaxStackZero_ThrowsNamingTheField()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ItemCatalogValidator.Validate(FileName, [_potion with { MaxStack = 0 }]));

        Assert.Equal("items.json: item 'potion': maxStack must be at least 1 (was 0).", exception.Message);
    }

    [Fact]
    public void Validate_MenuItemTargetingEnemies_ThrowsNamingTheItem()
    {
        var bomb = _potion with { Id = "bomb", Effect = new Effect(EffectKind.Damage, Power: 15, Targeting.SingleEnemy) };

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [bomb]));

        Assert.StartsWith("items.json: item 'bomb': isUsableFromMenu", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_EffectAndEquipment_ThrowsNamingTheItem()
    {
        var hybrid = _potion with { Equipment = new EquipmentStats(EquipmentSlot.Weapon, 1, 0) };

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [hybrid]));

        Assert.Equal("items.json: item 'potion': an item has either an effect or equipment, not both.", exception.Message);
    }

    [Fact]
    public void Validate_NegativeEffectPower_ThrowsNamingTheField()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ItemCatalogValidator.Validate(FileName, [_potion with { Effect = _potion.Effect! with { Power = -1 } }]));

        Assert.Equal("items.json: item 'potion': effect.power must be at least 0 (was -1).", exception.Message);
    }

    [Fact]
    public void Validate_ReviveWithZeroPower_ThrowsNamingTheField()
    {
        var feather = _potion with { Id = "revive_feather", Effect = new Effect(EffectKind.Revive, Power: 0, Targeting.AllFallenAllies) };

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [feather]));

        Assert.Equal("items.json: item 'revive_feather': effect.power must be at least 1 for a revive effect (was 0).", exception.Message);
    }

    [Fact]
    public void Validate_NegativeDefenseBonus_ThrowsNamingTheField()
    {
        var armor = _potion with
        {
            Id = "leather_armor",
            IsUsableFromMenu = false,
            Effect = null,
            Equipment = new EquipmentStats(EquipmentSlot.Armor, AttackBonus: 0, DefenseBonus: -3),
        };

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [armor]));

        Assert.Equal("items.json: item 'leather_armor': equipment.defenseBonus must be at least 0 (was -3).", exception.Message);
    }

    [Fact]
    public void Validate_NegativeAttackBonus_ThrowsNamingTheField()
    {
        var sword = Equipment(new EquipmentStats(EquipmentSlot.Weapon, AttackBonus: -5, DefenseBonus: 0));

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [sword]));

        Assert.Equal("items.json: item 'gear': equipment.attackBonus must be at least 0 (was -5).", exception.Message);
    }

    [Fact]
    public void Validate_EmptyId_ThrowsNamingTheEntry()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ItemCatalogValidator.Validate(FileName, [_potion with { Id = " " }]));

        Assert.Equal("items.json: entry 0 has an empty id.", exception.Message);
    }

    private static ItemDefinition Equipment(EquipmentStats stats) =>
        _potion with { Id = "gear", IsUsableFromMenu = false, Effect = null, Equipment = stats };
}
