using Jrpg.Core.Crafting;
using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Session;

public sealed class GameSessionForgeTests
{
    // The default rules hold no salvage row, so a yield proves the session's Forge reads the catalog's.
    [Fact]
    public void Forge_CatalogWithSalvageRules_QuotesTheirYield()
    {
        var scrap = new ItemDefinitionBuilder().WithId("scrap").AsMaterial().Build();
        var sword = new ItemDefinitionBuilder().WithId("sword").AsWeapon(5).Build();
        var catalog = new ContentCatalogBuilder().WithMembers(new PartyMemberBuilder().WithId("sarxian").BuildDefinition()).WithItems(scrap, sword).WithLeader("sarxian").Build();
        var salvage = new Dictionary<Rarity, IReadOnlyList<InventoryEntry>> { [Rarity.Common] = [new("scrap", 4)] };
        catalog = catalog with { Rules = catalog.Rules with { Crafting = new CraftingRules { Salvage = salvage } } };

        var session = GameSession.StartNew(catalog, "sarxian", new FixedSequenceRandom());

        Assert.Equal([new InventoryEntry("scrap", 4)], session.Forge.QuoteSalvage(GearPiece.Plain(sword)).Materials);
    }
}
