using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

/// <summary>
/// Renaming a map's id with every reference to it, on a copy of the test world (decision 530, ADR 0042): start warps
/// into the forest, the new game starts on the intro map.
/// </summary>
public sealed class ContentWriterRenameTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("writer-rename", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterRenameTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void RenameMap_ValidId_MovesBothFiles()
    {
        _writer.RenameMap(_catalog, "forest", "woods");

        Assert.True(File.Exists(LayoutPath("woods")));
        Assert.True(File.Exists(SettingsPath("woods")));
        Assert.False(File.Exists(LayoutPath("forest")));
        Assert.False(File.Exists(SettingsPath("forest")));
        var woods = ContentCatalogLoader.Load(_content.Path).Maps["woods"];
        Assert.Equal(("woods", "woods"), (woods.Layout.Id, woods.Settings.Id));
    }

    [Fact]
    public void RenameMap_WarpsFromAnotherMap_FollowTheNewId()
    {
        var cellsBefore = WarpsInto(_catalog.Maps["start"].Layout, "forest").Select(warp => warp.Cell).ToList();
        Assert.NotEmpty(cellsBefore);

        _writer.RenameMap(_catalog, "forest", "woods");

        var start = ContentCatalogLoader.Load(_content.Path).Maps["start"].Layout;
        Assert.Equal(cellsBefore, WarpsInto(start, "woods").Select(warp => warp.Cell));
        Assert.Empty(WarpsInto(start, "forest"));
    }

    [Fact]
    public void RenameMap_WarpToItself_FollowsTheNewId()
    {
        var forest = _catalog.Maps["forest"];
        var loop = new CellEvent(new GridCell(0, 0), new MapEvent { Id = "loop", Activation = EventActivation.Touch, Steps = [new WarpStep("forest", new GridCell(1, 1))] });
        var withLoop = MapContent.With(_catalog, new MapDefinition(forest.Layout with { CellEvents = [.. forest.Layout.CellEvents, loop] }, forest.Settings));

        _writer.RenameMap(withLoop, "forest", "woods");

        var woods = ContentCatalogLoader.Load(_content.Path).Maps["woods"].Layout;
        Assert.Contains(WarpsInto(woods, "woods"), warp => warp.Cell == new GridCell(1, 1));
    }

    [Fact]
    public void RenameMap_StartMap_NewGameFollows()
    {
        Assert.Equal("intro", _catalog.NewGame.StartLocation.MapId);

        _writer.RenameMap(_catalog, "intro", "prologue");

        var loaded = ContentCatalogLoader.Load(_content.Path);
        Assert.Equal("prologue", loaded.NewGame.StartLocation.MapId);
        Assert.True(loaded.Maps.ContainsKey("prologue"));
    }

    [Fact]
    public void RenameMap_OtherMap_LeavesNewGameUntouched()
    {
        var before = File.ReadAllBytes(_content.PathOf("new-game.json"));

        _writer.RenameMap(_catalog, "forest", "woods");

        Assert.Equal(before, File.ReadAllBytes(_content.PathOf("new-game.json")));
    }

    [Fact]
    public void RenameMap_UnrelatedMap_LeavesItsLayoutUntouched()
    {
        Assert.Empty(WarpsInto(_catalog.Maps["interior_1"].Layout, "forest"));
        var before = File.ReadAllBytes(LayoutPath("interior_1"));

        _writer.RenameMap(_catalog, "forest", "woods");

        Assert.Equal(before, File.ReadAllBytes(LayoutPath("interior_1")));
    }

    // The taken id's own files are what a wrong write would overwrite.
    [Fact]
    public void RenameMap_NewIdTaken_WritesNothing()
    {
        var forestBefore = File.ReadAllBytes(LayoutPath("forest"));
        var startBefore = File.ReadAllBytes(LayoutPath("start"));

        Assert.Throws<ArgumentException>(() => _writer.RenameMap(_catalog, "forest", "start"));

        Assert.Equal(forestBefore, File.ReadAllBytes(LayoutPath("forest")));
        Assert.Equal(startBefore, File.ReadAllBytes(LayoutPath("start")));
    }

    [Fact]
    public void RenameMap_ContentInvalid_WritesNothingAndKeepsTheOldFiles()
    {
        var forest = _catalog.Maps["forest"];
        var ground = forest.Layout.Layers[0];
        IReadOnlyList<int> row = [_catalog.Tiles.Keys.Max() + 1, .. ground.Rows[0].Skip(1)];
        var broken = MapContent.With(_catalog, new MapDefinition(forest.Layout with { Layers = [ground with { Rows = [row, .. ground.Rows.Skip(1)] }, .. forest.Layout.Layers.Skip(1)] }, forest.Settings));

        Assert.Throws<ContentValidationException>(() => _writer.RenameMap(broken, "forest", "woods"));

        Assert.True(File.Exists(LayoutPath("forest")));
        Assert.False(File.Exists(LayoutPath("woods")));
    }

    [Fact]
    public void RenameMap_Renamed_LoadsBack()
    {
        _writer.RenameMap(_catalog, "forest", "woods");

        var loaded = ContentCatalogLoader.Load(_content.Path);

        Assert.True(loaded.Maps.ContainsKey("woods"));
        Assert.False(loaded.Maps.ContainsKey("forest"));
    }

    private static IEnumerable<WarpStep> WarpsInto(MapLayout layout, string mapId) =>
        layout.CellEvents.Select(cellEvent => cellEvent.Event)
            .Concat(layout.Objects.SelectMany(placed => placed.Events))
            .SelectMany(mapEvent => mapEvent.Steps.OfType<WarpStep>())
            .Where(warp => warp.MapId == mapId);

    private string LayoutPath(string mapId) => Path.Combine(_content.Path, "map-layouts", $"{mapId}.json");

    private string SettingsPath(string mapId) => Path.Combine(_content.Path, "maps", $"{mapId}.json");
}
