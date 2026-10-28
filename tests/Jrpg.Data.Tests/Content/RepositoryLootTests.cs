using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The loot in the repository's content (the loot plan): the goblin's drop table (D2) and the affix pool (R3a).
public sealed class RepositoryLootTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void Goblin_DropsFromATableThatExists()
    {
        var tableId = _catalog.Enemies["goblin"].DropTableId;

        Assert.NotNull(tableId);
        Assert.NotEmpty(_catalog.DropTables[tableId].Entries);
    }

    [Fact]
    public void AffixPool_HasPowerForWeapons()
    {
        var weaponStats = _catalog.Affixes.Values.Where(affix => affix.Kinds.Contains(EquipmentKind.Weapon)).Select(affix => affix.Stat);

        Assert.Contains(StatKind.Power, weaponStats);
    }
}
