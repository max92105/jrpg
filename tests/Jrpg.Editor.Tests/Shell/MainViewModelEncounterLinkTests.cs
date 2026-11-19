using Jrpg.Editor.Encounters;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// The Encounters page's links through the shell on the test world: a link is a step on its map at once, on a map the
/// session hadn't opened too, ordered among the other steps so Ctrl+Z reaches it once that map is shown.
/// </summary>
public sealed class MainViewModelEncounterLinkTests : IDisposable
{
    private readonly EditorTestContent _content = new();
    private readonly MainViewModel _viewModel;
    private readonly EncounterCatalogViewModel _page;
    private readonly string _encounterId;
    private readonly string _mapId;

    public MainViewModelEncounterLinkTests()
    {
        _viewModel = _content.Open(_content.TestWorld);
        _page = _viewModel.EncounterEditing!.Start();
        _encounterId = _page.SelectedEncounter!.Id;

        // A map with random battles that don't pick the encounter and that isn't the shown one.
        _mapId = _viewModel.Catalog!.Maps.Values
            .First(map => map.Settings.RandomEncounters is { } table
                && !table.EncounterIds.Contains(_encounterId)
                && map.Id != _viewModel.CurrentMap?.Map.Id)
            .Id;
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void LinkMap_MapNotOpen_ChangesItUnsavedWithoutShowingIt()
    {
        var shown = _viewModel.CurrentMap;

        Link();

        Assert.True(_viewModel.HasUnsavedChanges);
        Assert.Same(shown, _viewModel.CurrentMap);
        Assert.Contains(_page.LinkedMaps, map => map.MapId == _mapId);
        var row = _viewModel.MapEntries.Single(entry => entry.Id == _mapId);
        Assert.EndsWith(" *", row.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void Undo_OnceTheLinkedMapIsShown_TakesTheLinkBackAndThePageUnlistsIt()
    {
        Link();
        _viewModel.TryOpenMap(_mapId);

        _viewModel.History!.UndoCommand.Execute(null);

        Assert.DoesNotContain(_encounterId, _viewModel.CurrentMap!.Map.Settings.RandomEncounters!.EncounterIds);
        Assert.DoesNotContain(_page.LinkedMaps, map => map.MapId == _mapId);
        Assert.False(_viewModel.HasUnsavedChanges);
    }

    // The shown map's step reaches the undo order by itself, so the shell must not order it a second time.
    [Fact]
    public void LinkMap_ShownMap_IsOneUndoStep()
    {
        _viewModel.TryOpenMap(_mapId);
        Link();

        _viewModel.History!.UndoCommand.Execute(null);

        Assert.False(_viewModel.History.UndoCommand.CanExecute(null));
        Assert.DoesNotContain(_encounterId, _viewModel.CurrentMap!.Map.Settings.RandomEncounters!.EncounterIds);
    }

    [Fact]
    public void UnlinkMap_LinkedMap_TakesTheEncounterOutOfItsBattles()
    {
        Link();
        _page.SelectedLinkedMap = _page.LinkedMaps.Single(map => map.MapId == _mapId);

        _page.UnlinkMapCommand.Execute(null);

        _viewModel.TryOpenMap(_mapId);
        Assert.DoesNotContain(_encounterId, _viewModel.CurrentMap!.Map.Settings.RandomEncounters!.EncounterIds);
    }

    private void Link()
    {
        var link = _page.StartLinkingMap();
        link.SelectedMap = link.Maps.Single(map => map.MapId == _mapId);
        _page.LinkMap(link);
    }
}
