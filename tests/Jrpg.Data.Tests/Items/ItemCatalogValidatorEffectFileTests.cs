using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using Jrpg.Data.Items;

namespace Jrpg.Data.Tests.Items;

// The strip an item names for itself (decisions 706, 707): a name when present, and only on an item that damages.
public sealed class ItemCatalogValidatorEffectFileTests
{
    private const string Rule = "effectFile is only for a damaging effect (decision 707).";

    private static readonly ItemDefinition _bomb = new()
    {
        Id = "bomb",
        Name = "Bomb",
        Description = "Damages.",
        IconFile = "Item.png",
        MaxStack = 99,
        IsUsableFromMenu = false,
        Effect = new Effect(EffectKind.Damage, Power: 15, Targeting.SingleEnemy, DamageType: DamageType.Fire),
    };

    [Fact]
    public void Validate_DamagingItemNamingAStrip_DoesNotThrow()
    {
        Assert.Null(Record.Exception(() => Validate(_bomb with { EffectFile = "Burst.png" })));
    }

    [Fact]
    public void Validate_BlankEffectFile_ThrowsNamingTheField()
    {
        var exception = Assert.Throws<ContentValidationException>(() => Validate(_bomb with { EffectFile = " " }));

        Assert.Equal("items.json: item 'bomb': effectFile must name a strip in effects/ when present.", exception.Message);
    }

    [Fact]
    public void Validate_HealingItemNamingAStrip_ThrowsNamingTheRule()
    {
        var healing = new Effect(EffectKind.RestoreHp, Power: 15, Targeting.SingleAlly);
        var potion = _bomb with { Effect = healing, EffectFile = "Burst.png" };

        var exception = Assert.Throws<ContentValidationException>(() => Validate(potion));

        Assert.Equal($"items.json: item 'bomb': {Rule}", exception.Message);
    }

    // No effect at all: the rule's other side.
    [Fact]
    public void Validate_WeaponNamingAStrip_ThrowsNamingTheRule()
    {
        var sword = _bomb with
        {
            Effect = null,
            Equipment = new EquipmentStats(EquipmentKind.Weapon) { Damage = 1, DamageType = DamageType.Slashing },
            EffectFile = "Burst.png",
        };

        var exception = Assert.Throws<ContentValidationException>(() => Validate(sword));

        Assert.Equal($"items.json: item 'bomb': {Rule}", exception.Message);
    }

    // A valid material passes the item's own rules, so the strip check after them still runs.
    [Fact]
    public void Validate_MaterialNamingAStrip_ThrowsNamingTheRule()
    {
        var scrap = _bomb with { Effect = null, IsMaterial = true, EffectFile = "Burst.png" };

        var exception = Assert.Throws<ContentValidationException>(() => Validate(scrap));

        Assert.Equal($"items.json: item 'bomb': {Rule}", exception.Message);
    }

    private static void Validate(ItemDefinition item) => ItemCatalogValidator.Validate("items.json", [item]);
}
