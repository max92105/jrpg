using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Content;

public sealed class ContentCatalogTests
{
    // A save keeps only the affix's id and value (ADR 0029); the pool says it raises Agility, not the id.
    [Fact]
    public void PieceFrom_Affix_TakesItsStatFromThePool()
    {
        var boots = new ItemDefinitionBuilder().WithId("leather_boots").AsWorn(EquipmentKind.Feet).Build();
        var swift = new AffixDefinition { Id = "swift", Stat = StatKind.Agility, Min = 1, Max = 5, Kinds = [EquipmentKind.Feet], Weight = 10 };
        var catalog = new ContentCatalogBuilder().WithItems(boots).WithAffixes(swift).Build();

        var piece = catalog.PieceFrom(new SavedGear("leather_boots", Rarity.Magic, [new SavedAffix("swift", 3)]));

        Assert.Equal([new RolledAffix("swift", StatKind.Agility, 3)], piece.Affixes);
        Assert.Equal(3, piece.Bonuses.Of(StatKind.Agility));
    }
}
