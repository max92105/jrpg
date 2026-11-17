using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// The maps whose random battles roll an encounter (decision 907), on the test world's start and forest maps given
// battles of the tests' own.
public sealed class CatalogUsageEncounterTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    // Forest after start, out of id order.
    [Fact]
    public void UsersOfEncounter_TwoMapsRollIt_ListsBothInIdOrder()
    {
        var content = WithMaps(Rolling("start", "test_wisps"), Rolling("forest", "test_wisps"));

        Assert.Equal(["map forest", "map start"], CatalogUsage.UsersOfEncounter(content, "test_wisps"));
    }

    [Fact]
    public void UsersOfEncounter_MapWithoutBattles_NotListed()
    {
        var start = _catalog.Maps["start"];
        var calm = new MapDefinition(start.Layout, start.Settings with { RandomEncounters = null });
        var content = WithMaps(calm, Rolling("forest", "test_wisps"));

        Assert.Equal(["map forest"], CatalogUsage.UsersOfEncounter(content, "test_wisps"));
    }

    [Fact]
    public void UsersOfEncounter_BattlesRollAnother_NotListed()
    {
        var content = WithMaps(Rolling("forest", "test_bats"), Rolling("start", "test_wisps"));

        Assert.Equal(["map start"], CatalogUsage.UsersOfEncounter(content, "test_wisps"));
    }

    private static MapDefinition Rolling(string mapId, string encounterId)
    {
        var map = _catalog.Maps[mapId];
        var battles = new RandomEncounterTable { OneInSteps = 20, GraceSteps = 3, EncounterIds = ["goblin_trio", encounterId] };
        return new MapDefinition(map.Layout, map.Settings with { RandomEncounters = battles });
    }

    // The catalog with only these maps, in the order given.
    private static ContentCatalog WithMaps(params MapDefinition[] maps) =>
        _catalog with { Maps = maps.ToDictionary(map => map.Id, StringComparer.Ordinal) };
}
