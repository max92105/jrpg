using Jrpg.Core.Geometry;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>Undo across switched maps: each open map keeps its steps while another is shown (decision 503).</summary>
public sealed class MainViewModelSwitchUndoTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Undo_AfterSwitchingAwayAndBack_UndoesTheMapsLastPaint()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var before = viewModel.CurrentMap!.Map.Layout.Layers[0].TileIdAt(new GridCell(0, 0));
        EditorTestContent.PaintTopLeft(viewModel);
        viewModel.TryOpenMap("start");
        viewModel.TryOpenMap("forest");

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal(before, viewModel.CurrentMap!.Map.Layout.Layers[0].TileIdAt(new GridCell(0, 0)));
        Assert.False(viewModel.CurrentMap.IsDirty);
    }

    [Fact]
    public void Undo_OnAnotherMap_DoesNotTouchTheHiddenMap()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var forest = viewModel.CurrentMap!;
        var painted = EditorTestContent.PaintTopLeft(viewModel);

        viewModel.TryOpenMap("start");
        viewModel.History!.UndoCommand.Execute(null);

        Assert.False(viewModel.History.UndoCommand.CanExecute(null));
        Assert.Equal(painted, forest.Map.Layout.Layers[0].TileIdAt(new GridCell(0, 0)));
    }

    [Fact]
    public void Redo_UndoneThenSwitchedAwayAndBack_RedoesThePaint()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var painted = EditorTestContent.PaintTopLeft(viewModel);
        viewModel.History!.UndoCommand.Execute(null);
        viewModel.TryOpenMap("start");
        viewModel.TryOpenMap("forest");

        viewModel.History.RedoCommand.Execute(null);

        Assert.Equal(painted, viewModel.CurrentMap!.Map.Layout.Layers[0].TileIdAt(new GridCell(0, 0)));
    }

    [Fact]
    public void UndoCanExecute_SwitchedToAMapWithSteps_IsRaised()
    {
        var viewModel = _content.Open(_content.TestWorld);
        EditorTestContent.PaintTopLeft(viewModel);
        viewModel.TryOpenMap("start");
        var raised = 0;
        viewModel.History!.UndoCommand.CanExecuteChanged += (_, _) => raised++;

        viewModel.TryOpenMap("forest");

        Assert.True(raised > 0);
        Assert.True(viewModel.History.UndoCommand.CanExecute(null));
    }
    // Forest places the object again after both undos; a redo from start would remove it from under forest.
    [Fact]
    public void Redo_ObjectRemovalUndoneWithItsPlacementOnAHiddenMap_CannotExecute()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        _content.ChooseTwoCellsNotTiles(viewModel);
        EditorTestContent.ComposeObject(viewModel, "test_box");
        var placing = viewModel.Placing!;
        placing.PlaceObjectAt(new Point2D(2, 2));
        placing.RemoveObject(viewModel.CurrentMap!.Map.Layout.Objects.Single(placed => placed.ObjectId == "test_box").Uid);
        placing.Browser.SelectedObject = placing.Browser.Objects.First(definition => definition.Id == "test_box");
        placing.Browser.RemoveSelectedCommand.Execute(null);
        viewModel.History!.UndoCommand.Execute(null);
        viewModel.History.UndoCommand.Execute(null);

        viewModel.TryOpenMap("start");

        Assert.False(viewModel.History.RedoCommand.CanExecute(null));
    }
}
