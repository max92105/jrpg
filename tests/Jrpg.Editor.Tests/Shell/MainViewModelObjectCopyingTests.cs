using Jrpg.Core.Geometry;
using Jrpg.Data.Content;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// Copy and Paste through the editor: across open maps, and Paste following the catalog (decision 512, B1).
/// </summary>
public sealed class MainViewModelObjectCopyingTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    // A forest object with events copied, pasted on start: fresh ids there, and start still saves.
    [Fact]
    public void PasteCenteredAt_AnotherMapOpened_PastesThereWithFreshIds()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var withEvents = viewModel.CurrentMap!.Map.Layout.Objects.First(placed => placed.Events.Count > 0);
        var placing = viewModel.Placing!;
        placing.Select([withEvents.Uid]);
        placing.Copying.CopyCommand.Execute(null);
        viewModel.TryOpenMap("start");
        var start = viewModel.CurrentMap!.Map.Layout;
        var takenIds = start.Objects.Select(placed => placed.Uid).Concat(start.AllEvents.Select(mapEvent => mapEvent.Id)).ToHashSet();

        placing.Copying.PasteCenteredAt(new Point2D(3.5, 3.5));
        viewModel.SaveCommand.Execute(null);

        var pasted = viewModel.CurrentMap.Map.Layout.Objects[^1];
        Assert.Equal(withEvents.ObjectId, pasted.ObjectId);
        Assert.DoesNotContain(pasted.Events.Select(mapEvent => mapEvent.Id).Append(pasted.Uid), takenIds.Contains);
        Assert.Null(viewModel.SaveError);
        Assert.Contains(ContentCatalogLoader.Load(_content.ContentCopy).Maps["start"].Layout.Objects, placed => placed.Uid == pasted.Uid);
    }

    // B1: a copied object whose composing is undone (its placement first) can't be pasted.
    [Fact]
    public void CanPaste_CopiedObjectsComposingUndone_IsFalseAndRaised()
    {
        var viewModel = CopyANewlyComposedObject();
        var changed = new List<string?>();
        viewModel.Placing!.Copying.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        viewModel.History!.UndoCommand.Execute(null);
        viewModel.History.UndoCommand.Execute(null);

        Assert.False(viewModel.Placing.Copying.CanPaste);
        Assert.Contains(nameof(ObjectCopying.CanPaste), changed);
    }

    [Fact]
    public void CanPaste_ComposingUndoneThenRedone_IsTrueAgain()
    {
        var viewModel = CopyANewlyComposedObject();
        viewModel.History!.UndoCommand.Execute(null);
        viewModel.History.UndoCommand.Execute(null);

        viewModel.History.RedoCommand.Execute(null);

        Assert.True(viewModel.Placing!.Copying.CanPaste);
    }

    // Objects > New object makes test_box, placed on forest at (2, 2), selected and copied.
    private Editor.Shell.MainViewModel CopyANewlyComposedObject()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        _content.ChooseTwoCellsNotTiles(viewModel);
        EditorTestContent.ComposeObject(viewModel, "test_box");
        var placing = viewModel.Placing!;
        placing.PlaceObjectAt(new Point2D(2, 2));
        placing.Select([viewModel.CurrentMap!.Map.Layout.Objects[^1].Uid]);
        placing.Copying.CopyCommand.Execute(null);
        return viewModel;
    }
}
