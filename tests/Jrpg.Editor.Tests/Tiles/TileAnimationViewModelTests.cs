using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Shell;
using Jrpg.Editor.Tests.Shell;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

// The Animate panel (decision 467) on the test world's forest sheet: 32 columns of 16 px cells, so columns 0 to 31.
public sealed class TileAnimationViewModelTests : IDisposable
{
    private const string Forest = "fg_forest_fall";

    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Open_AnimatedTile_StartsWithItsCountAndTime()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var tile = viewModel.Catalog!.Tiles.Values.First(tile => tile.SheetId == Forest);
        viewModel.Content!.SetTileAnimations(new Dictionary<int, TileAnimation?> { [tile.Id] = new TileAnimation { FrameCount = 6, FrameMilliseconds = 90 } });
        viewModel.Painting!.Browser.Select(tile.Id);

        viewModel.Animating!.OpenCommand.Execute(null);

        Assert.Equal(("6", "90"), (viewModel.Animating.FrameCountText, viewModel.Animating.FrameMillisecondsText));
        Assert.Equal(new SheetCell(Forest, tile.Column, tile.Row), viewModel.Animating.Cell);
    }

    [Fact]
    public void Open_CellNotATileYet_StartsWithTheDefaults()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        _content.ChooseCellNotATile(viewModel);

        viewModel.Animating!.OpenCommand.Execute(null);

        Assert.Equal(("4", "150"), (viewModel.Animating.FrameCountText, viewModel.Animating.FrameMillisecondsText));
    }

    // A city cell chosen with City.png gone (decision 442).
    [Fact]
    public void CanOpen_CellOfASheetWithoutItsImage_IsFalse()
    {
        var root = _content.CopyOfTestWorld();
        File.Delete(Path.Combine(ArtFolders.RootIn(root), ArtFolders.Tilesheets, "City.png"));
        var viewModel = _content.Open(root);

        viewModel.Painting!.Browser.Select(viewModel.Catalog!.Tiles.Values.First(tile => tile.SheetId == "city").Id);

        Assert.False(viewModel.Animating!.CanOpen);
    }

    [Fact]
    public void CanOpen_NothingChosen_IsFalse() => Assert.False(_content.Open(_content.TestWorld).Animating!.CanOpen);

    [Fact]
    public void CanOpen_CellOfASetUpSheet_IsTrueAndRaised()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var raised = new List<string?>();
        viewModel.Animating!.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        ChooseForestCell(viewModel, 3, 2);

        Assert.True(viewModel.Animating.CanOpen);
        Assert.Contains(nameof(TileAnimationViewModel.CanOpen), raised);
    }

    [Theory]
    [InlineData("2", null)]
    [InlineData("1", "Frames must be a whole number from 2.")]
    [InlineData("x", "Frames must be a whole number from 2.")]
    public void Problem_FrameCounts(string typed, string? problem)
    {
        var animating = OpenOnForestCell(0, 0);

        animating.FrameCountText = typed;

        Assert.Equal(problem, animating.Problem);
    }

    [Theory]
    [InlineData("1", null)]
    [InlineData("0", "The time must be a whole number of milliseconds from 1.")]
    [InlineData("-5", "The time must be a whole number of milliseconds from 1.")]
    [InlineData("x", "The time must be a whole number of milliseconds from 1.")]
    public void Problem_FrameTimes(string typed, string? problem)
    {
        var animating = OpenOnForestCell(0, 0);

        animating.FrameMillisecondsText = typed;

        Assert.Equal(problem, animating.Problem);
    }

    [Fact]
    public void Problem_LastFrameOnTheLastColumn_IsNull()
    {
        var animating = OpenOnForestCell(28, 0);

        animating.FrameCountText = "4";

        Assert.Null(animating.Problem);
    }

    [Fact]
    public void Problem_LastFramePastTheLastColumn_SaysSo()
    {
        var animating = OpenOnForestCell(28, 0);

        animating.FrameCountText = "5";

        Assert.Equal("5 frames from column 28 run past the sheet's 32 columns.", animating.Problem);
    }

    // At column 3, 3 + 2147483647 wraps to a negative sum: the check must not add.
    [Fact]
    public void Problem_FrameCountNearTheIntLimit_SaysItRunsPast()
    {
        var animating = OpenOnForestCell(3, 0);

        animating.FrameCountText = "2147483647";

        Assert.Equal("2147483647 frames from column 3 run past the sheet's 32 columns.", animating.Problem);
    }

    [Fact]
    public void Close_OpenPanel_IsNotOpen()
    {
        var animating = OpenOnForestCell(3, 2);
        Assert.True(animating.IsOpen);

        animating.CloseCommand.Execute(null);

        Assert.False(animating.IsOpen);
        Assert.Null(animating.Cell);
    }

    [Fact]
    public void Open_SheetRemovedFromTheCatalog_Closes()
    {
        var viewModel = _content.Open(_content.TestWorld);
        ChooseForestCell(viewModel, 3, 2);
        viewModel.Animating!.OpenCommand.Execute(null);

        viewModel.Content!.RemoveTilesheet(Forest);

        Assert.False(viewModel.Animating.IsOpen);
    }

    [Fact]
    public void PreviewCellAt_AtTheFrameTime_IsTheNextCellToTheRight()
    {
        var animating = OpenOnForestCell(3, 2);

        Assert.Equal(new SheetCell(Forest, 4, 2), animating.PreviewCellAt(TimeSpan.FromMilliseconds(TileAnimationViewModel.DefaultFrameMilliseconds)));
    }

    [Fact]
    public void PreviewCellAt_WithAProblem_IsTheOpenedCell()
    {
        var animating = OpenOnForestCell(3, 2);
        animating.FrameCountText = "1";

        Assert.Equal(new SheetCell(Forest, 3, 2), animating.PreviewCellAt(TimeSpan.FromMilliseconds(TileAnimationViewModel.DefaultFrameMilliseconds)));
    }

    private static void ChooseForestCell(MainViewModel viewModel, int column, int row)
    {
        var browser = viewModel.Painting!.Browser;
        browser.SelectedChoice = browser.SheetChoices.First(choice => choice.Sheet?.Id == Forest);
        var sheet = browser.SelectedSheet!;
        browser.SelectCellAt(sheet.CellLeft(column), sheet.CellTop(row));
    }

    private TileAnimationViewModel OpenOnForestCell(int column, int row)
    {
        var viewModel = _content.Open(_content.TestWorld);
        ChooseForestCell(viewModel, column, row);
        viewModel.Animating!.OpenCommand.Execute(null);
        return viewModel.Animating;
    }
}
