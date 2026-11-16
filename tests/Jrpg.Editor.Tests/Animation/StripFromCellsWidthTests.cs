using Jrpg.Data.Art;
using Jrpg.Editor.Animation;

namespace Jrpg.Editor.Tests.Animation;

// Strip from sheet cells' OK greyed on a strip wider than the game loads (decision 990, TextureLimit): each pick adds a
// square of the cell's larger side, so the width is known from the boxes before anything is written.
public sealed class StripFromCellsWidthTests
{
    // A sheet one pixel high is guessed as a row of 1 px squares, so a click on the row's number picks every pixel.
    [Fact]
    public void Problem_StripAtTheTextureLimit_IsNone()
    {
        var tool = new StripFromCellsViewModel();
        tool.TakeSheet("line.png", (TextureLimit.MaxSide, 1));

        tool.PickRow(0);

        Assert.Equal((TextureLimit.MaxSide, null, true), (tool.Picks.Count, tool.Problem, tool.CanWrite));
    }

    [Fact]
    public void Problem_StripOnePixelPastTheLimit_SaysItsWidth()
    {
        var tool = new StripFromCellsViewModel();
        tool.TakeSheet("line.png", (TextureLimit.MaxSide + 1, 1));

        tool.PickRow(0);

        var expected = $"The strip would be {TextureLimit.MaxSide + 1} px wide; the game takes at most {TextureLimit.MaxSide}.";
        Assert.Equal((expected, false), (tool.Problem, tool.CanWrite));
    }

    // One cell 100 px wide and one pixel taller than the limit: padded to a square of its height, the strip is as wide
    // as the cell is high.
    [Fact]
    public void Problem_OneCellTallerThanTheLimit_SaysItsPaddedWidth()
    {
        var tool = new StripFromCellsViewModel();
        tool.TakeSheet("tall.png", (100, TextureLimit.MaxSide + 1));

        tool.PickCellAt(0, 0);

        Assert.Equal(
            $"The strip would be {TextureLimit.MaxSide + 1} px wide; the game takes at most {TextureLimit.MaxSide}.",
            tool.Problem);
    }
}
