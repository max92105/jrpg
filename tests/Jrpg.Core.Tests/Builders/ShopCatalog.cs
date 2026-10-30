using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Core.Trade;

namespace Jrpg.Core.Tests.Builders;

/// <summary>
/// A catalog with one shop ("store": a potion, a sword and a ring), a Magic-only table, one weapon affix and one ring
/// affix, so each rolled piece draws exactly its affix's value, in the shop's order; the sell share is 50 %.
/// </summary>
internal static class ShopCatalog
{
    public static readonly ItemDefinition Potion = new ItemDefinitionBuilder().WithId("potion").WithPrice(20).Build();
    public static readonly ItemDefinition Sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(24).WithPrice(100).Build();
    public static readonly ItemDefinition Ring = new ItemDefinitionBuilder().WithId("power_ring").AsWorn(EquipmentKind.Ring).WithPrice(250).Build();
    public static readonly AffixDefinition Power = new() { Id = "power", Stat = StatKind.Power, Min = 2, Max = 5, Kinds = [EquipmentKind.Weapon], Weight = 10, GoldValue = 20 };
    public static readonly AffixDefinition Agility = new() { Id = "agility", Stat = StatKind.Agility, Min = 2, Max = 5, Kinds = [EquipmentKind.Ring], Weight = 10, GoldValue = 20 };

    public static ContentCatalog Build()
    {
        var magicOnly = new LootRules(new Dictionary<Rarity, RarityRule> { [Rarity.Magic] = new(Weight: 1, MinAffixes: 1, MaxAffixes: 1) });
        var catalog = new ContentCatalogBuilder()
            .WithMembers(new PartyMemberBuilder().WithId("sarxian").BuildDefinition())
            .WithItems(Potion, Sword, Ring)
            .WithAffixes(Power, Agility)
            .WithShops(new ShopDefinition("store", "Store", ["potion", "bronze_sword", "power_ring"]))
            .WithLeader("sarxian")
            .Build();
        return catalog with { Rules = catalog.Rules with { Loot = magicOnly, Trade = new TradeRules { SellPercent = 50 } } };
    }

    /// <summary>A Magic piece of <paramref name="item"/> with one affix at <paramref name="value"/>.</summary>
    public static GearPiece Magic(ItemDefinition item, AffixDefinition affix, int value) =>
        GearPiece.Rolled(item, Rarity.Magic, [new RolledAffix(affix.Id, affix.Stat, value)]);
}
