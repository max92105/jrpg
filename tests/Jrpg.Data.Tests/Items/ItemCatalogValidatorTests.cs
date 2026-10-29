using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Stats;
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
    public void Validate_RestItemNotUsableFromMenu_Throws()
    {
        var kit = _potion with { Id = "camp_kit", IsUsableFromMenu = false, Effect = new Effect(EffectKind.Rest, Power: 0, Targeting.AllAllies) };

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [kit]));

        Assert.Equal(
            "items.json: item 'camp_kit': effect.kind rest needs isUsableFromMenu true and effect.targeting allAllies; a rest is used from the menu on the whole party.",
            exception.Message);
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
        var bomb = _potion with { Id = "bomb", Effect = new Effect(EffectKind.Damage, Power: 15, Targeting.SingleEnemy, DamageType: DamageType.Fire) };

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [bomb]));

        Assert.StartsWith("items.json: item 'bomb': isUsableFromMenu", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_ItemWithAlliesInRange_Throws()
    {
        var charm = _potion with { Id = "charm", Effect = new Effect(EffectKind.RestoreHp, Power: 15, Targeting.AlliesInRange, Range: 1) };

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [charm]));

        Assert.Equal("items.json: item 'charm': effect.targeting alliesInRange is for skills only; an item has no battle formation to reach along.", exception.Message);
    }

    [Fact]
    public void Validate_ItemThatReadiesATurn_Throws()
    {
        var hourglass = _potion with { Id = "hourglass", Effect = new Effect(EffectKind.ReadyExtraTurn, Power: 0, Targeting.OtherCombatant) };

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [hourglass]));

        Assert.Equal("items.json: item 'hourglass': effect.kind readyExtraTurn and effect.targeting otherCombatant are for skills only; an item has no turn order.", exception.Message);
    }

    [Fact]
    public void Validate_ItemAimedAtOtherCombatant_Throws()
    {
        var dart = _potion with { Id = "dart", Effect = new Effect(EffectKind.Damage, Power: 5, Targeting.OtherCombatant, DamageType: DamageType.Fire) };

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [dart]));

        Assert.EndsWith("effect.kind readyExtraTurn and effect.targeting otherCombatant are for skills only; an item has no turn order.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_EffectAndEquipment_ThrowsNamingTheItem()
    {
        var hybrid = _potion with { Equipment = new EquipmentStats(EquipmentKind.Weapon) { Damage = 1 } };

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
    public void Validate_WeaponWithoutDamage_NamesTheField()
    {
        var sword = Equipment(new EquipmentStats(EquipmentKind.Weapon) { DamageType = DamageType.Slashing });

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [sword]));

        Assert.Equal("items.json: item 'gear': equipment.damage must be at least 1 for a weapon (was 0).", exception.Message);
    }

    [Fact]
    public void Validate_ArmorWithDamage_NamesTheField()
    {
        var armor = Equipment(new EquipmentStats(EquipmentKind.Chest) { Damage = 5 });

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [armor]));

        Assert.Equal("items.json: item 'gear': equipment.damage is only for a weapon (was 5 on Chest).", exception.Message);
    }

    [Fact]
    public void Validate_WeaponWithoutDamageType_NamesTheField()
    {
        var sword = Equipment(new EquipmentStats(EquipmentKind.Weapon) { Damage = 5 });

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [sword]));

        Assert.Equal("items.json: item 'gear': equipment.damageType is required for a weapon.", exception.Message);
    }

    [Fact]
    public void Validate_ArmorWithDamageType_NamesTheField()
    {
        var armor = Equipment(new EquipmentStats(EquipmentKind.Chest) { DamageType = DamageType.Slashing });

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [armor]));

        Assert.Equal("items.json: item 'gear': equipment.damageType is only for a weapon (was Slashing on Chest).", exception.Message);
    }

    [Fact]
    public void Validate_ZeroBonus_NamesTheStat()
    {
        var ring = Equipment(new EquipmentStats(EquipmentKind.Chest) { Bonuses = Bonuses((StatKind.Power, 5), (StatKind.FireResistance, 0)) });

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [ring]));

        Assert.Equal("items.json: item 'gear': equipment.bonuses.fireResistance must be nonzero and within -100..100 (was 0).", exception.Message);
    }

    [Fact]
    public void Validate_BonusAbove100_NamesTheStat()
    {
        var ring = Equipment(new EquipmentStats(EquipmentKind.Chest) { Bonuses = Bonuses((StatKind.Power, 101)) });

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [ring]));

        Assert.Equal("items.json: item 'gear': equipment.bonuses.power must be nonzero and within -100..100 (was 101).", exception.Message);
    }

    // A base item never gives what only a roll may (ADR 0030); each affix-only stat adds its case.
    [Theory]
    [InlineData(StatKind.LifeSteal, "lifeSteal")]
    [InlineData(StatKind.ArmorPenetration, "armorPenetration")]
    [InlineData(StatKind.BlockChance, "blockChance")]
    public void Validate_BonusOnAffixOnlyStat_Throws(StatKind kind, string name)
    {
        var ring = Equipment(new EquipmentStats(EquipmentKind.Ring) { Bonuses = Bonuses((kind, 5)) });

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [ring]));

        Assert.Equal($"items.json: item 'gear': equipment.bonuses.{name} is a stat only affixes give (ADR 0030).", exception.Message);
    }

    [Fact]
    public void Validate_NegativeBonusWithinBounds_Passes()
    {
        var cursed = Equipment(new EquipmentStats(EquipmentKind.Chest) { Bonuses = Bonuses((StatKind.Agility, -100)) });

        var exception = Record.Exception(() => ItemCatalogValidator.Validate(FileName, [cursed]));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_EmptyId_ThrowsNamingTheEntry()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ItemCatalogValidator.Validate(FileName, [_potion with { Id = " " }]));

        Assert.Equal("items.json: entry 0 has an empty id.", exception.Message);
    }

    private static StatBonuses Bonuses(params (StatKind Kind, int Bonus)[] entries) =>
        new(entries.ToDictionary(entry => entry.Kind, entry => entry.Bonus));

    private static ItemDefinition Equipment(EquipmentStats stats) =>
        _potion with { Id = "gear", IsUsableFromMenu = false, Effect = null, Equipment = stats };
}
