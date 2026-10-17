using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Data.Content;
using Jrpg.Data.Items;

namespace Jrpg.Data.Tests.Items;

public sealed class ItemCatalogValidatorTests
{
    private const string FileName = "items.json";

    private static readonly HashSet<string> _classIds = ["warrior"];

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
        var exception = Record.Exception(() => ItemCatalogValidator.Validate(FileName, [_potion, _potion with { Id = "hi_potion" }], _classIds));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_DuplicateId_ThrowsNamingTheId()
    {
        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [_potion, _potion], _classIds));

        Assert.Equal("items.json: item id 'potion' is used more than once.", exception.Message);
    }

    [Fact]
    public void Validate_NullEntry_ThrowsNamingTheIndex()
    {
        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [_potion, null!], _classIds));

        Assert.Equal("items.json: entry 1 is null.", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmptyName_ThrowsNamingTheField(string name)
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ItemCatalogValidator.Validate(FileName, [_potion with { Name = name }], _classIds));

        Assert.Equal("items.json: item 'potion': name must not be empty.", exception.Message);
    }

    [Fact]
    public void Validate_MaxStackZero_ThrowsNamingTheField()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ItemCatalogValidator.Validate(FileName, [_potion with { MaxStack = 0 }], _classIds));

        Assert.Equal("items.json: item 'potion': maxStack must be at least 1 (was 0).", exception.Message);
    }

    [Fact]
    public void Validate_MenuItemTargetingEnemies_ThrowsNamingTheItem()
    {
        var bomb = _potion with { Id = "bomb", Effect = new Effect(EffectKind.Damage, Power: 15, Targeting.SingleEnemy) };

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [bomb], _classIds));

        Assert.StartsWith("items.json: item 'bomb': isUsableFromMenu", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_EffectAndEquipment_ThrowsNamingTheItem()
    {
        var hybrid = _potion with { Equipment = new EquipmentStats(EquipmentSlot.Weapon, 1, 0, ["warrior"]) };

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [hybrid], _classIds));

        Assert.Equal("items.json: item 'potion': an item has either an effect or equipment, not both.", exception.Message);
    }

    [Fact]
    public void Validate_NegativeEffectPower_ThrowsNamingTheField()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ItemCatalogValidator.Validate(FileName, [_potion with { Effect = _potion.Effect! with { Power = -1 } }], _classIds));

        Assert.Equal("items.json: item 'potion': effect.power must be at least 0 (was -1).", exception.Message);
    }

    [Fact]
    public void Validate_ReviveWithZeroPower_ThrowsNamingTheField()
    {
        var feather = _potion with { Id = "revive_feather", Effect = new Effect(EffectKind.Revive, Power: 0, Targeting.AllFallenAllies) };

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [feather], _classIds));

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
            Equipment = new EquipmentStats(EquipmentSlot.Armor, AttackBonus: 0, DefenseBonus: -3, ["warrior"]),
        };

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [armor], _classIds));

        Assert.Equal("items.json: item 'leather_armor': equipment.defenseBonus must be at least 0 (was -3).", exception.Message);
    }

    [Fact]
    public void Validate_NegativeAttackBonus_ThrowsNamingTheField()
    {
        var sword = Equipment(new EquipmentStats(EquipmentSlot.Weapon, AttackBonus: -5, DefenseBonus: 0, ["warrior"]));

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [sword], _classIds));

        Assert.Equal("items.json: item 'gear': equipment.attackBonus must be at least 0 (was -5).", exception.Message);
    }

    [Fact]
    public void Validate_NullAllowedClassId_ThrowsNamingTheField()
    {
        var sword = Equipment(new EquipmentStats(EquipmentSlot.Weapon, AttackBonus: 5, DefenseBonus: 0, [null!]));

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [sword], _classIds));

        Assert.Equal("items.json: item 'gear': equipment.allowedClassIds must not contain a null or empty class id.", exception.Message);
    }

    [Fact]
    public void Validate_UnknownAllowedClass_ThrowsNamingTheItem()
    {
        var staff = Equipment(new EquipmentStats(EquipmentSlot.Weapon, AttackBonus: 6, DefenseBonus: 0, ["warrior", "necromancer"]));

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [staff], _classIds));

        Assert.Equal("items.json: item 'gear': equipment.allowedClassIds has unknown class id 'necromancer'.", exception.Message);
    }

    [Fact]
    public void Validate_EmptyId_ThrowsNamingTheEntry()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ItemCatalogValidator.Validate(FileName, [_potion with { Id = " " }], _classIds));

        Assert.Equal("items.json: entry 0 has an empty id.", exception.Message);
    }

    private static ItemDefinition Equipment(EquipmentStats stats) =>
        _potion with { Id = "gear", IsUsableFromMenu = false, Effect = null, Equipment = stats };
}
