using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The loot in the repository's content (the loot plan): the goblin's drop table (D2), the affix pool (R3a, R3b).
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

    // A Magic or Rare piece rolls among its kind's affixes; a kind with none would drop a Magic piece with no line.
    [Fact]
    public void GoblinGearDrops_EachHasAffixesForItsKind()
    {
        var gear = _catalog.DropTables[_catalog.Enemies["goblin"].DropTableId!].Entries
            .Select(entry => _catalog.Items[entry.ItemId].Equipment)
            .OfType<EquipmentStats>();

        Assert.NotEmpty(gear);
        Assert.All(gear, equipment => Assert.Contains(_catalog.Affixes.Values, affix => affix.Kinds.Contains(equipment.Kind)));
    }

    // Block shows only on an off-hand that drops (decision 366): the goblins drop one, and the pool rolls block on it.
    [Fact]
    public void GoblinDrops_IncludeAnOffHandThatCanRollBlock()
    {
        var dropKinds = _catalog.DropTables[_catalog.Enemies["goblin"].DropTableId!].Entries
            .Select(entry => _catalog.Items[entry.ItemId].Equipment?.Kind);

        Assert.Contains(EquipmentKind.OffHand, dropKinds);
        Assert.Contains(_catalog.Affixes.Values, affix => affix.Stat == StatKind.BlockChance && affix.Kinds.Contains(EquipmentKind.OffHand));
    }
}
