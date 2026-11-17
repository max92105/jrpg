using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// The maps whose events give an item (decision 883), saved or open as they are now, on the test world's start map.
public sealed class CatalogUsageItemTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly MapDefinition _start = _catalog.Maps["start"];

    [Fact]
    public void UsersOfItem_SavedMapGivesIt_NamesTheMap()
    {
        var giving = new MapDefinition(Giving(_start.Layout, "test_tonic"), _start.Settings);
        var saved = _catalog with { Maps = new Dictionary<string, MapDefinition>(_catalog.Maps) { ["start"] = giving } };

        Assert.Equal(["map start"], CatalogUsage.UsersOfItem(saved, "test_tonic", []));
    }

    // The saved start map doesn't give it; the open one, unsaved, does.
    [Fact]
    public void UsersOfItem_OpenMapsUnsavedStepGivesIt_NamesTheMap()
    {
        Assert.Equal(["map start"], CatalogUsage.UsersOfItem(_catalog, "test_tonic", [Giving(_start.Layout, "test_tonic")]));
    }

    [Fact]
    public void UsersOfItem_StepGivesAnotherItem_IsEmpty()
    {
        Assert.Empty(CatalogUsage.UsersOfItem(_catalog, "test_tonic", [Giving(_start.Layout, "test_elixir")]));
    }

    private static MapLayout Giving(MapLayout layout, string itemId)
    {
        var mapEvent = new MapEvent
        {
            Id = "gives",
            Activation = EventActivation.Touch,
            Steps = [new GiveItemStep(itemId, 1)],
        };
        return layout with { CellEvents = [.. layout.CellEvents, new CellEvent(new GridCell(0, 0), mapEvent)] };
    }
}
