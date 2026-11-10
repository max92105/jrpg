using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Tests.Maps;

/// <summary>
/// A map id renamed through the content (decision 530), on the test world: start's cell events warp into the forest and
/// its door into the interior; the new game starts on the intro map.
/// </summary>
public sealed class MapIdRenameTests
{
    private static readonly ContentCatalog _testWorld = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    [Fact]
    public void Apply_CellEventWarp_Renamed()
    {
        var renamed = MapIdRename.Apply(_testWorld, "forest", "woods");

        var start = renamed.Content.Maps["start"].Layout;
        Assert.Contains(start.CellEvents, cellEvent => cellEvent.Event.Steps.Any(step => step is WarpStep { MapId: "woods" }));
        Assert.DoesNotContain(start.CellEvents, cellEvent => cellEvent.Event.Steps.Any(step => step is WarpStep { MapId: "forest" }));
        Assert.Contains("start", renamed.ChangedMapIds);
        Assert.DoesNotContain("interior_1", renamed.ChangedMapIds);
    }

    [Fact]
    public void Apply_ObjectEventWarp_Renamed()
    {
        var renamed = MapIdRename.Apply(_testWorld, "interior_1", "shop");

        var doors = renamed.Content.Maps["start"].Layout.Objects.SelectMany(placed => placed.Events).SelectMany(mapEvent => mapEvent.Steps).OfType<WarpStep>();
        Assert.Contains(doors, warp => warp.MapId == "shop");
        Assert.DoesNotContain(doors, warp => warp.MapId == "interior_1");
    }

    // The door's warp into the interior is neither rebuilt nor touched when the forest is renamed.
    [Fact]
    public void Apply_WarpToAnotherMap_Untouched()
    {
        var doorBefore = _testWorld.Maps["start"].Layout.Objects.First(placed => placed.Events.Any(mapEvent => mapEvent.Steps.Any(step => step is WarpStep { MapId: "interior_1" })));

        var renamed = MapIdRename.Apply(_testWorld, "forest", "woods");

        var doorAfter = renamed.Content.Maps["start"].Layout.Objects.Single(placed => placed.Uid == doorBefore.Uid);
        Assert.Equal(doorBefore.Events, doorAfter.Events);
        Assert.Same(_testWorld.Maps["interior_1"], renamed.Content.Maps["interior_1"]);
    }

    [Fact]
    public void Apply_RenamedMap_HasTheNewIdInBothRecords()
    {
        var renamed = MapIdRename.Apply(_testWorld, "forest", "woods");

        var woods = renamed.Content.Maps["woods"];
        Assert.Equal(("woods", "woods"), (woods.Layout.Id, woods.Settings.Id));
        Assert.False(renamed.Content.Maps.ContainsKey("forest"));
        Assert.False(renamed.IsNewGameChanged);
    }

    [Fact]
    public void Apply_StartMap_NewGameFollows()
    {
        var renamed = MapIdRename.Apply(_testWorld, "intro", "prologue");

        Assert.Equal("prologue", renamed.Content.NewGame.StartLocation.MapId);
        Assert.True(renamed.IsNewGameChanged);
    }

    [Fact]
    public void Apply_UnknownOldId_Throws() => Assert.Throws<ArgumentException>(() => MapIdRename.Apply(_testWorld, "nowhere", "woods"));

    [Fact]
    public void Apply_NewIdTaken_Throws() => Assert.Throws<ArgumentException>(() => MapIdRename.Apply(_testWorld, "forest", "start"));

    // Ids are file names, which Windows compares without case: "Start" would overwrite start's files.
    [Fact]
    public void Apply_NewIdTakenWithOtherCase_Throws() => Assert.Throws<ArgumentException>(() => MapIdRename.Apply(_testWorld, "forest", "Start"));
}
