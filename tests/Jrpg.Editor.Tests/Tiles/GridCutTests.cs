using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

/// <summary>
/// The grid rule the tile Cut panel and the sprite setup share: the cases the panel's tests pinned, now direct.
/// </summary>
public sealed class GridCutTests
{
    [Fact]
    public void LengthAlong_EvenSplit_IsTheCellLength() => Assert.Equal(16, GridCut.Plain.LengthAlong(128, 8));

    [Fact]
    public void LengthAlong_NoCells_IsZero() => Assert.Equal(0, GridCut.Plain.LengthAlong(128, 0));

    [Fact]
    public void ProblemAlong_UnevenSplit_NamesPixelsAndCells() =>
        Assert.Equal("100 px doesn't split into 3 columns.", GridCut.Plain.ProblemAlong(100, 3, "columns"));

    [Fact]
    public void ProblemAlong_MoreCellsThanPixels_SaysTooSmall() =>
        Assert.Equal("4 px is too small for 5 rows.", GridCut.Plain.ProblemAlong(4, 5, "rows"));

    [Fact]
    public void ProblemAlong_EvenSplit_IsNull() => Assert.Null(GridCut.Plain.ProblemAlong(128, 8, "columns"));

    // 2 px around the grid and 1 px between 4 cells: 100 - 4 - 3 = 93 px for the cells, 23 each with 1 over.
    [Fact]
    public void LengthAlong_MarginAndSpacing_LeavesThemOut()
    {
        var cut = new GridCut(Margin: 2, Spacing: 1);

        Assert.Equal(23, cut.LengthAlong(100, 4));
        Assert.Equal("100 px doesn't split into 4 columns.", cut.ProblemAlong(100, 4, "columns"));
    }
}
