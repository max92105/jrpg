using Jrpg.Core.Maps;
using Jrpg.Editor.Shell;
using Jrpg.Editor.Tests.Shell;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

// The Animate panel's Apply and Remove animation (decision 467), each one catalog step, on a copy of the test world.
public sealed class TileAnimationApplyTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Apply_CellNotATileYet_MakesTheTileWithTheAnimation()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var cell = _content.ChooseCellNotATile(viewModel);
        var animating = Opened(viewModel);

        animating.ApplyCommand.Execute(null);

        Assert.Equal(new TileAnimation { FrameCount = 4, FrameMilliseconds = 150 }, viewModel.Content!.TileAt(cell)?.Animation);
        Assert.False(animating.IsOpen);
    }

    [Fact]
    public void Apply_AnimatedTile_ReplacesItsCountAndTime()
    {
        var (viewModel, tile) = OpenOnAnimatedTile();
        var animating = viewModel.Animating!;
        animating.FrameCountText = "2";
        animating.FrameMillisecondsText = "70";

        animating.ApplyCommand.Execute(null);

        Assert.Equal(new TileAnimation { FrameCount = 2, FrameMilliseconds = 70 }, viewModel.Catalog!.Tiles[tile.Id].Animation);
    }

    [Fact]
    public void Apply_WithAProblem_CannotExecute()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        _content.ChooseCellNotATile(viewModel);
        var animating = Opened(viewModel);

        animating.FrameCountText = "1";

        Assert.False(animating.ApplyCommand.CanExecute(null));
    }

    [Fact]
    public void Apply_Undo_MakesTheTileStillAgain()
    {
        var (viewModel, tile) = OpenOnStillTile();
        viewModel.Animating!.ApplyCommand.Execute(null);
        Assert.NotNull(viewModel.Catalog!.Tiles[tile.Id].Animation);

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Null(viewModel.Catalog!.Tiles[tile.Id].Animation);
    }

    [Fact]
    public void RemoveAnimation_AnimatedTile_LeavesItStill()
    {
        var (viewModel, tile) = OpenOnAnimatedTile();

        viewModel.Animating!.RemoveAnimationCommand.Execute(null);

        Assert.Null(viewModel.Catalog!.Tiles[tile.Id].Animation);
    }

    [Fact]
    public void RemoveAnimation_StillTile_CannotExecute()
    {
        var (viewModel, _) = OpenOnStillTile();

        Assert.False(viewModel.Animating!.RemoveAnimationCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveAnimation_AnimationUndoneWhileOpen_CannotExecute()
    {
        var (viewModel, _) = OpenOnStillTile();
        viewModel.Animating!.FrameCountText = "2";
        viewModel.Animating.ApplyCommand.Execute(null);
        viewModel.Animating.OpenCommand.Execute(null);
        Assert.True(viewModel.Animating.RemoveAnimationCommand.CanExecute(null));

        viewModel.History!.UndoCommand.Execute(null);

        Assert.False(viewModel.Animating.RemoveAnimationCommand.CanExecute(null));
    }

    [Fact]
    public void FrameCountText_Typed_RaisesApplysCanExecuteChanged()
    {
        var (viewModel, _) = OpenOnStillTile();
        var raised = 0;
        viewModel.Animating!.ApplyCommand.CanExecuteChanged += (_, _) => raised++;

        viewModel.Animating.FrameCountText = "1";

        Assert.Equal(1, raised);
    }

    private static TileAnimationViewModel Opened(MainViewModel viewModel)
    {
        viewModel.Animating!.OpenCommand.Execute(null);
        return viewModel.Animating;
    }

    // A forest tile far enough from the right edge for the default 4 frames.
    private (MainViewModel ViewModel, TileDefinition Tile) OpenOnStillTile()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var tile = viewModel.Catalog!.Tiles.Values.First(tile => tile.SheetId == "fg_forest_fall" && tile.Column <= 28);
        viewModel.Painting!.Browser.Select(tile.Id);
        Opened(viewModel);
        return (viewModel, tile);
    }

    private (MainViewModel ViewModel, TileDefinition Tile) OpenOnAnimatedTile()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var tile = viewModel.Catalog!.Tiles.Values.First(tile => tile.SheetId == "fg_forest_fall" && tile.Column <= 28);
        viewModel.Content!.SetTileAnimations(new Dictionary<int, TileAnimation?> { [tile.Id] = new TileAnimation { FrameCount = 3, FrameMilliseconds = 100 } });
        viewModel.Painting!.Browser.Select(tile.Id);
        Opened(viewModel);
        return (viewModel, tile);
    }
}
