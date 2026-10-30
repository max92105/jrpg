using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Session;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;
using Jrpg.Core.Trade;

namespace Jrpg.Core.Tests.Session;

public sealed class GameSessionTradeTests
{
    // The default rules pay nothing, so gold in the purse proves the session's sales read the catalog's share and the
    // affixes' gold values.
    [Fact]
    public void Sales_SellOwnedPiece_GoldReachesThePartysPurse()
    {
        var sword = new ItemDefinitionBuilder().WithId("sword").AsWeapon(5).WithPrice(100).Build();
        var power = new AffixDefinition { Id = "power", Stat = StatKind.Power, Min = 2, Max = 5, Kinds = [EquipmentKind.Weapon], Weight = 10, GoldValue = 20 };
        var catalog = new ContentCatalogBuilder().WithMembers(new PartyMemberBuilder().WithId("sarxian").BuildDefinition()).WithItems(sword).WithLeader("sarxian").Build();
        catalog = catalog with { Rules = catalog.Rules with { Trade = new TradeRules { SellPercent = 50 } }, Affixes = new Dictionary<string, AffixDefinition> { ["power"] = power } };
        var session = GameSession.StartNew(catalog, "sarxian", new FixedSequenceRandom());
        var piece = GearPiece.Rolled(sword, Rarity.Magic, [new RolledAffix("power", StatKind.Power, 4)]);
        session.Inventory.AddGear(piece);
        var before = session.Party.Purse.Gold;

        var sold = session.Sales.TrySell(piece);

        Assert.True(sold);
        Assert.Equal(before + 60, session.Party.Purse.Gold);
    }
}
