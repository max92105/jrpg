using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Data.Content;
using Jrpg.Data.Items;

namespace Jrpg.Data.Tests.Items;

// An item names no tiered effect: it isn't raised by its user, so a potion heals its number (decision 785).
public sealed class ItemCatalogValidatorTierTests
{
    [Fact]
    public void Validate_ItemEffectNamingATieredEffect_RefusesNamingTheItem()
    {
        var potion = new ItemDefinition
        {
            Id = "potion",
            Name = "Potion",
            Description = "Restores HP.",
            IconFile = "Consumable.png",
            MaxStack = 99,
            IsUsableFromMenu = true,
            Effect = new Effect(EffectKind.RestoreHp, Power: 0, Targeting.SingleAlly)
            {
                Tier = new TieredEffect("heal_1", EffectKind.RestoreHp, 2),
            },
        };

        var exception = Assert.Throws<ContentValidationException>(() => Validate([potion]));

        Assert.Equal(
            "items.json: item 'potion': effect.tier is for skills: an item isn't raised by its user (decision 785).",
            exception.Message);
    }

    private static void Validate(ItemDefinition[] items) => ItemCatalogValidator.Validate("items.json", items, AffixPools.None);
}
