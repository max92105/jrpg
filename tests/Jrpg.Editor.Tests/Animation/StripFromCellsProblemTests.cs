using System.ComponentModel;
using Jrpg.Core.Sprites;
using Jrpg.Editor.Animation;
using static Jrpg.Editor.Tests.Animation.StripFromCellsViewModelTests;

namespace Jrpg.Editor.Tests.Animation;

// What greys Strip from sheet cells' OK, what OK writes and what the tool raises for its window (decision 990), on
// StripFromCellsViewModelTests' 6 x 2 cells of 32 x 32 px.
public sealed class StripFromCellsProblemTests
{
    [Fact]
    public void Problem_NoSheet_AsksForOne()
    {
        var tool = new StripFromCellsViewModel();

        Assert.Equal("Open a sheet.", tool.Problem);
    }

    [Fact]
    public void Problem_NoPick_AsksForThem()
    {
        var tool = SixByTwo();

        Assert.Equal("Pick the cells the strip plays, in order.", tool.Problem);
    }

    [Fact]
    public void Problem_ColumnsNotSplitting_IsTheCutMessage()
    {
        var tool = Picked();

        tool.ColumnsText = "5";

        Assert.Equal("192 px doesn't split into 5 columns.", tool.Problem);
    }

    [Fact]
    public void Problem_RowsNotSplitting_IsTheCutMessage()
    {
        var tool = Picked();

        tool.RowsText = "3";

        Assert.Equal("64 px doesn't split into 3 rows.", tool.Problem);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("six")]
    public void Problem_ColumnsNotACount_SaysSo(string columns)
    {
        var tool = Picked();

        tool.ColumnsText = columns;

        Assert.Equal("The columns must be a whole number from 1.", tool.Problem);
    }

    [Theory]
    [InlineData("slash/hit")]
    [InlineData("slash\\hit")]
    public void Problem_NameWithASlash_SaysSo(string name)
    {
        var tool = Picked();

        tool.StripNameText = name;

        Assert.Equal($"The strip's name can't hold '{name[5]}'.", tool.Problem);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData(".png")]
    public void Problem_NoName_AsksForOne(string name)
    {
        var tool = Picked();

        tool.StripNameText = name;

        Assert.Equal("Name the strip.", tool.Problem);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("fast")]
    public void Problem_FrameTimeNotFromOne_SaysSo(string frameTime)
    {
        var tool = Picked();

        tool.FrameMillisecondsText = frameTime;

        Assert.Equal(("Frame time must be a whole number from 1.", false), (tool.Problem, tool.CanWrite));
    }

    [Fact]
    public void Problem_FrameTimeOfOne_IsNone()
    {
        var tool = Picked();

        tool.FrameMillisecondsText = "1";

        Assert.Equal((null, true), (tool.Problem, tool.CanWrite));
    }

    [Fact]
    public void FrameMilliseconds_New_IsTheUnlistedPace()
    {
        var tool = new StripFromCellsViewModel();

        Assert.Equal(80, tool.FrameMilliseconds);
    }

    [Fact]
    public void ToStrip_TypedNameWithoutExtension_AddsPng()
    {
        var tool = Picked();

        tool.StripNameText = " slash_hit ";

        Assert.Equal("slash_hit.png", tool.ToStrip().FileName);
    }

    [Fact]
    public void ToStrip_TypedNameWithExtension_KeepsIt()
    {
        var tool = Picked();

        tool.StripNameText = "slash_hit.PNG";

        Assert.Equal("slash_hit.PNG", tool.ToStrip().FileName);
    }

    [Fact]
    public void ToStrip_PicksAndPace_CarriesThem()
    {
        var tool = Picked();
        tool.PickCellAt(0, 0);
        tool.FrameMillisecondsText = "120";

        var strip = tool.ToStrip();

        Assert.Equal((32, 32), strip.CellSize);
        Assert.Equal([new SpriteCell(1, 1), new SpriteCell(0, 0)], strip.Cells);
        Assert.Equal(120, strip.FrameMilliseconds);
    }

    [Fact]
    public void ToStrip_OnAProblem_ThrowsIt()
    {
        var tool = SixByTwo();

        var thrown = Assert.Throws<InvalidOperationException>(() => tool.ToStrip());

        Assert.Equal("Pick the cells the strip plays, in order.", thrown.Message);
    }

    [Fact]
    public void PicksChanged_APick_IsRaised()
    {
        var tool = SixByTwo();
        var raised = 0;
        tool.PicksChanged += (_, _) => raised++;

        tool.PickCellAt(0, 0);

        Assert.Equal(1, raised);
    }

    [Fact]
    public void PicksChanged_ACutHoldingThePicks_IsRaisedOnce()
    {
        var tool = Picked();
        var raised = 0;
        tool.PicksChanged += (_, _) => raised++;

        tool.ColumnsText = "3";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void PicksChanged_ACutClearingThePicks_IsRaisedOnce()
    {
        var tool = Picked();
        var raised = 0;
        tool.PicksChanged += (_, _) => raised++;

        tool.RowsText = "1";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void PicksChanged_SameCutTyped_IsNotRaised()
    {
        var tool = Picked();
        var raised = 0;
        tool.PicksChanged += (_, _) => raised++;

        tool.ColumnsText = "6";

        Assert.Equal(0, raised);
    }

    [Fact]
    public void Picks_APick_RaisesTheLineAndProblem()
    {
        var tool = SixByTwo();
        var names = new List<string?>();
        ((INotifyPropertyChanged)tool).PropertyChanged += (_, e) => names.Add(e.PropertyName);

        tool.PickCellAt(0, 0);

        Assert.Equal(
            [nameof(tool.PicksLine), nameof(tool.Problem), nameof(tool.CanWrite)],
            names.Intersect([nameof(tool.PicksLine), nameof(tool.Problem), nameof(tool.CanWrite)]));
    }

    [Fact]
    public void Picks_APick_RaisesRemoveLastAndClearCanExecute()
    {
        var tool = SixByTwo();
        var raised = (RemoveLast: 0, Clear: 0);
        tool.RemoveLastCommand.CanExecuteChanged += (_, _) => raised.RemoveLast++;
        tool.ClearCommand.CanExecuteChanged += (_, _) => raised.Clear++;

        tool.PickCellAt(0, 0);

        Assert.Equal((1, 1), raised);
    }

    [Fact]
    public void ColumnsText_Changed_RaisesTheCellSizeAndProblem()
    {
        var tool = SixByTwo();
        var names = new List<string?>();
        ((INotifyPropertyChanged)tool).PropertyChanged += (_, e) => names.Add(e.PropertyName);

        tool.ColumnsText = "3";

        Assert.Equal(
            [nameof(tool.CellSize), nameof(tool.CutLine), nameof(tool.Problem)],
            names.Intersect([nameof(tool.CellSize), nameof(tool.CutLine), nameof(tool.Problem)]));
    }

    // A cut that doesn't split the sheet greys OK; a row picked meanwhile would add cells of no real size.
    [Fact]
    public void PickRow_CutNotSplitting_AddsNothing()
    {
        var tool = SixByTwo();
        tool.ColumnsText = "5";

        tool.PickRow(0);

        Assert.Empty(tool.Picks);
    }

    // One pick, cell (2, 2) as the sheet numbers it.
    private static StripFromCellsViewModel Picked()
    {
        var tool = SixByTwo();
        tool.PickCellAt(40, 40);
        return tool;
    }
}
