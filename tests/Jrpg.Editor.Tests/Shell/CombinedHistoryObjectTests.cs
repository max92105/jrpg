using Jrpg.Core.Events;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Shell;

// An undo that would take away an object a saved map's event swaps to is refused naming the map, as a placement is
// (decision 117): the save would refuse the swap to an unknown object.
public sealed class CombinedHistoryObjectTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Undo_NewObjectAnEventSwapsTo_IsRefusedNamingTheMap()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        _content.ChooseTwoCellsNotTiles(viewModel);
        EditorTestContent.ComposeObject(viewModel, "tree_4");
        var map = viewModel.CurrentMap!;
        // An object with no events yet, so the new interact event isn't shadowed by an earlier one that always starts.
        var bare = map.Map.Layout.Objects.First(placed => placed.Events.Count == 0);
        var swapping = new MapEvent { Id = $"{bare.Uid}_0", Activation = EventActivation.Interact, Prompt = "Swap", Steps = [new SwapObjectStep("tree_4")] };
        map.Do(new SetObjectEventsEdit(bare.Uid, bare.Events, [swapping]));
        viewModel.SaveCommand.Execute(null);
        Assert.Null(viewModel.SaveError);
        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != map.Map.Id));

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: object tree_4 is used on map {map.Map.Id}.", viewModel.History.Refusal);
        Assert.True(viewModel.Catalog!.Objects.ContainsKey("tree_4"));
    }

    // The removal undone, then another map opened: the closed map may use the object again since, so no redo is left.
    [Fact]
    public void Redo_RemovalUndoneThenAnotherMapOpened_CannotExecute()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        _content.ChooseTwoCellsNotTiles(viewModel);
        EditorTestContent.ComposeObject(viewModel, "tree_4");
        var browser = viewModel.Placing!.Browser;
        browser.SelectedObject = browser.Objects.First(definition => definition.Id == "tree_4");
        browser.RemoveSelectedCommand.Execute(null);
        viewModel.History!.UndoCommand.Execute(null);
        Assert.True(viewModel.History.RedoCommand.CanExecute(null));

        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != viewModel.CurrentMap!.Map.Id));

        Assert.False(viewModel.History.RedoCommand.CanExecute(null));
    }
}
