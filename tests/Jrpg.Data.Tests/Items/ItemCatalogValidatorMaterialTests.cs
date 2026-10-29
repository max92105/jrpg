using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Data.Content;
using Jrpg.Data.Items;

namespace Jrpg.Data.Tests.Items;

// A material is only spent at the forge (decision 339): never used, worn, or picked from the menu.
public sealed class ItemCatalogValidatorMaterialTests
{
    private const string FileName = "items.json";

    private static readonly ItemDefinition _scrap = new()
    {
        Id = "scrap",
        Name = "Scrap",
        Description = "Metal scraps.",
        IconFile = "Item.png",
        MaxStack = 99,
        IsUsableFromMenu = false,
        IsMaterial = true,
    };

    [Fact]
    public void Validate_Material_DoesNotThrow()
    {
        var exception = Record.Exception(() => ItemCatalogValidator.Validate(FileName, [_scrap]));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_MaterialWithEffect_Throws()
    {
        var material = _scrap with { Effect = new Effect(EffectKind.RestoreHp, Power: 15, Targeting.SingleAlly) };

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [material]));

        Assert.Equal("items.json: item 'scrap': a material (isMaterial true) has no effect.", exception.Message);
    }

    [Fact]
    public void Validate_MaterialWithEquipment_Throws()
    {
        var material = _scrap with { Equipment = new EquipmentStats(EquipmentKind.Chest) };

        var exception = Assert.Throws<ContentValidationException>(() => ItemCatalogValidator.Validate(FileName, [material]));

        Assert.Equal("items.json: item 'scrap': a material (isMaterial true) has no equipment.", exception.Message);
    }

    [Fact]
    public void Validate_MaterialUsableFromMenu_Throws()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ItemCatalogValidator.Validate(FileName, [_scrap with { IsUsableFromMenu = true }]));

        Assert.Equal("items.json: item 'scrap': a material (isMaterial true) needs isUsableFromMenu false.", exception.Message);
    }
}
