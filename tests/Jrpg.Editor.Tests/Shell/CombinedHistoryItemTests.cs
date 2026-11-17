using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Shell;

// An undo that would take away an item a map's event gives is refused naming the map (decision 883, as decision 581
// refuses an opened shop); a refused undo leaves the item and its step in place.
public sealed class CombinedHistoryItemTests : IDisposable
{
    private static readonly ItemDefinition _tonic = new()
    {
        Id = "test_tonic",
        Name = "Test Tonic",
        Description = "A tonic only these tests know.",
        IconFile = "Item.png",
        MaxStack = 99,
        IsUsableFromMenu = false,
        IsMaterial = true,
    };

    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Undo_RemovesAnItemAnEventGives_RefusedNamingTheMap()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        viewModel.Content!.SetItems([.. viewModel.Catalog!.Items.Values, _tonic]);
        var mapId = EditorTestContent.SaveWithCellEvent(viewModel, new GiveItemStep("test_tonic", 1));

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: item test_tonic is given on map {mapId}.", viewModel.History.Refusal);
        Assert.True(viewModel.Catalog.Items.ContainsKey("test_tonic"));
        Assert.True(viewModel.Content.CanUndo);
    }

    [Fact]
    public void Undo_RemovesAnItemNobodyGives_Undoes()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        viewModel.Content!.SetItems([.. viewModel.Catalog!.Items.Values, _tonic]);
        EditorTestContent.SaveWithCellEvent(viewModel, new GiveItemStep("potion", 1));

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Null(viewModel.History.Refusal);
        Assert.False(viewModel.Catalog.Items.ContainsKey("test_tonic"));
    }

    // The open map's unsaved step counts too: placed before the item, so the item's step is the next undo.
    [Fact]
    public void Undo_RemovesAnItemAnUnsavedEventGives_RefusedNamingTheMap()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var map = viewModel.CurrentMap!;
        var givesTonic = new MapEvent
        {
            Id = "item_check",
            Activation = EventActivation.Touch,
            Steps = [new GiveItemStep("test_tonic", 1)],
        };
        map.Do(SetCellEventEdit.For(map.Map.Layout.CellEvents, new GridCell(0, 0), givesTonic));
        viewModel.Content!.SetItems([.. viewModel.Catalog!.Items.Values, _tonic]);

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: item test_tonic is given on map {map.Map.Id}.", viewModel.History.Refusal);
        Assert.True(viewModel.Catalog.Items.ContainsKey("test_tonic"));
        Assert.True(viewModel.Content.CanUndo);
    }
}
