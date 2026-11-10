using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Maps;

/// <summary>A map list row from a map as it is now, its folder included (decision 532).</summary>
public sealed class MapEntryTests
{
    [Fact]
    public void Of_MapInAFolder_CarriesIt()
    {
        var entry = MapEntry.Of(Map("mill", "Act 1/Village"), isUnsaved: false);

        Assert.Equal(new MapEntry("mill", "mill (mill)", "Act 1/Village"), entry);
    }

    // The open map was moved and not saved yet: the tree shows it in its new folder at once, with the unsaved mark.
    [Fact]
    public void Of_OpenMapMovedUnsaved_TakesTheNewFolder()
    {
        var open = new MapViewModel(Map("mill", "Act 1"));
        open.Do(new SetMapSettingsEdit(open.Map.Settings, open.Map.Settings with { Folder = "Act 2" }));
        var maps = new OpenMaps();
        maps.Add(open);

        var entries = maps.EntriesWith(_testWorld with { Maps = new Dictionary<string, MapDefinition> { ["mill"] = Map("mill", "Act 1") } });

        Assert.Equal(new MapEntry("mill", "mill (mill) *", "Act 2"), Assert.Single(entries));
    }

    private static readonly ContentCatalog _testWorld = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private static MapDefinition Map(string id, string? folder) => new(
        new MapLayout { Id = id, Width = 1, Height = 1, Layers = [new TileLayer { Z = 0, Rows = [[0]] }], Objects = [] },
        new MapSettings { Id = id, DisplayName = id, Folder = folder });
}
