using Jrpg.Core.Sprites;
using Jrpg.Editor.Animation;

namespace Jrpg.Editor.Tests.Animation;

// Strip from sheet cells' cut and picks (decision 990), on a 192 x 64 px sheet cut in 6 x 2 cells of 32 x 32 px unless
// a test says otherwise.
public sealed class StripFromCellsViewModelTests
{
    [Fact]
    public void TakeSheet_SixSquaresInARow_GuessesSixByOne()
    {
        var tool = new StripFromCellsViewModel();

        tool.TakeSheet("slash.png", (192, 32));

        Assert.Equal(("6", "1"), (tool.ColumnsText, tool.RowsText));
    }

    [Fact]
    public void TakeSheet_NotSquares_GuessesOneByOne()
    {
        var tool = new StripFromCellsViewModel { ColumnsText = "4", RowsText = "3" };

        tool.TakeSheet("sheet.png", (100, 64));

        Assert.Equal(("1", "1"), (tool.ColumnsText, tool.RowsText));
    }

    [Fact]
    public void TakeSheet_AfterPicks_ClearsThemAndKeepsPicking()
    {
        var tool = SixByTwo();
        tool.PickCellAt(10, 10);

        tool.TakeSheet("other.png", (64, 32));

        Assert.Equal((0, true), (tool.Picks.Count, tool.IsPicking));
    }

    [Fact]
    public void TakeSheet_NoName_NamesTheStripAsTheSheet()
    {
        var tool = new StripFromCellsViewModel();

        tool.TakeSheet("burst_sheet.png", (192, 64));

        Assert.Equal("burst_sheet.png", tool.StripNameText);
    }

    [Fact]
    public void TakeSheet_NameTyped_KeepsIt()
    {
        var tool = new StripFromCellsViewModel { StripNameText = "slash" };

        tool.TakeSheet("sheet.png", (192, 64));

        Assert.Equal("slash", tool.StripNameText);
    }

    [Fact]
    public void CutLine_SixByTwo_NamesTheCellSize()
    {
        var tool = SixByTwo();

        Assert.Equal(("6 x 2 cells of 32 x 32 px", (32, 32)), (tool.CutLine, tool.CellSize));
    }

    [Fact]
    public void CutLine_ColumnsNotSplitting_IsEmptyWithNoCellSize()
    {
        var tool = SixByTwo();

        tool.ColumnsText = "5";

        Assert.Equal(("", null), (tool.CutLine, tool.CellSize));
    }

    [Fact]
    public void PickCellAt_PointInSecondColumn_AddsColumnTwo()
    {
        var tool = SixByTwo();

        tool.PickCellAt(40, 40);

        Assert.Equal([new SpriteCell(1, 1)], tool.Picks);
    }

    [Theory]
    [InlineData(31.9, 0)]
    [InlineData(32, 1)]
    [InlineData(191.9, 5)]
    public void PickCellAt_AroundACellEdge_AddsTheCellHoldingThePoint(double x, int column)
    {
        var tool = SixByTwo();

        tool.PickCellAt(x, 0);

        Assert.Equal([new SpriteCell(column, 0)], tool.Picks);
    }

    [Theory]
    [InlineData(192, 10)]
    [InlineData(10, 64)]
    [InlineData(-0.1, 10)]
    [InlineData(10, -0.1)]
    public void PickCellAt_OutsideTheSheet_AddsNothing(double x, double y)
    {
        var tool = SixByTwo();

        tool.PickCellAt(x, y);

        Assert.Empty(tool.Picks);
    }

    [Fact]
    public void PickCellAt_SameCellTwice_KeepsBoth()
    {
        var tool = SixByTwo();

        tool.PickCellAt(40, 10);
        tool.PickCellAt(50, 20);

        Assert.Equal([new SpriteCell(1, 0), new SpriteCell(1, 0)], tool.Picks);
    }

    [Fact]
    public void PickCellAt_NotPicking_AddsNothing()
    {
        var tool = SixByTwo();
        tool.IsPicking = false;

        tool.PickCellAt(40, 10);

        Assert.Empty(tool.Picks);
    }

    [Fact]
    public void PickCellAt_CutNotSplitting_AddsNothing()
    {
        var tool = SixByTwo();
        tool.ColumnsText = "5";

        tool.PickCellAt(40, 10);

        Assert.Empty(tool.Picks);
    }

    [Fact]
    public void PickCellAt_NoSheet_AddsNothing()
    {
        var tool = new StripFromCellsViewModel { IsPicking = true };

        tool.PickCellAt(0, 0);

        Assert.Empty(tool.Picks);
    }

    [Fact]
    public void PickRow_SecondRowOfSix_AddsItsSixCellsLeftToRight()
    {
        var tool = SixByTwo();

        tool.PickRow(1);

        Assert.Equal([.. Enumerable.Range(0, 6).Select(column => new SpriteCell(column, 1))], tool.Picks);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(-1)]
    public void PickRow_PastTheCut_AddsNothing(int row)
    {
        var tool = SixByTwo();

        tool.PickRow(row);

        Assert.Empty(tool.Picks);
    }

    [Fact]
    public void PickRow_NotPicking_AddsNothing()
    {
        var tool = SixByTwo();
        tool.IsPicking = false;

        tool.PickRow(0);

        Assert.Empty(tool.Picks);
    }

    [Fact]
    public void ColumnsChanged_PicksPastTheNewCut_Cleared()
    {
        var tool = SixByTwo();
        tool.PickCellAt(100, 40);

        tool.ColumnsText = "3";

        Assert.Empty(tool.Picks);
    }

    [Fact]
    public void RowsChanged_PicksPastTheNewCut_Cleared()
    {
        var tool = SixByTwo();
        tool.PickCellAt(100, 40);

        tool.RowsText = "1";

        Assert.Empty(tool.Picks);
    }

    [Fact]
    public void ColumnsChanged_PicksOnTheNewLastColumn_Kept()
    {
        var tool = SixByTwo();
        tool.PickCellAt(70, 40);

        tool.ColumnsText = "3";

        Assert.Equal([new SpriteCell(2, 1)], tool.Picks);
    }

    [Fact]
    public void ColumnsChanged_NotACount_KeepsThePicks()
    {
        var tool = SixByTwo();
        tool.PickCellAt(100, 40);

        tool.ColumnsText = "";

        Assert.Equal([new SpriteCell(3, 1)], tool.Picks);
    }

    [Fact]
    public void PicksLine_FourPicks_ListsThemFromOne()
    {
        var tool = SixByTwo();

        foreach (var x in new[] { 0, 32, 64, 32 })
        {
            tool.PickCellAt(x, 0);
        }

        Assert.Equal("4 cells: (1, 1) (2, 1) (3, 1) (2, 1)", tool.PicksLine);
    }

    [Fact]
    public void RemoveLast_TwoPicks_LeavesOne()
    {
        var tool = SixByTwo();
        tool.PickCellAt(0, 0);
        tool.PickCellAt(40, 0);

        tool.RemoveLastCommand.Execute(null);

        Assert.Equal([new SpriteCell(0, 0)], tool.Picks);
    }

    [Fact]
    public void Clear_TwoPicks_LeavesNone()
    {
        var tool = SixByTwo();
        tool.PickCellAt(0, 0);
        tool.PickCellAt(40, 0);

        tool.ClearCommand.Execute(null);

        Assert.Empty(tool.Picks);
    }

    [Fact]
    public void RemoveLastAndClear_NoPick_AreGreyed()
    {
        var tool = SixByTwo();

        Assert.Equal((false, false), (tool.RemoveLastCommand.CanExecute(null), tool.ClearCommand.CanExecute(null)));
    }

    // The sheet's guess is 3 x 1 squares of 64 px, so the cut is typed over it.
    internal static StripFromCellsViewModel SixByTwo()
    {
        var tool = new StripFromCellsViewModel();
        tool.TakeSheet("sheet.png", (192, 64));
        (tool.ColumnsText, tool.RowsText) = ("6", "2");
        return tool;
    }
}
