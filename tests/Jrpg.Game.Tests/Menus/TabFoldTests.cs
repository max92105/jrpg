using Jrpg.Game.Menus;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Menus;

public sealed class TabFoldTests
{
    [Fact]
    public void Share_New_IsUnfolded()
    {
        var fold = new TabFold();

        Assert.Equal(0, fold.Share);
    }

    // Eased out: halfway through the time, 1 - 0.5³ of the way there.
    [Fact]
    public void Share_HalfwayThroughFolding_IsEasedPastHalf()
    {
        var fold = new TabFold();
        fold.Fold();

        fold.Advance(TabFold.Duration / 2);

        Assert.Equal(0.875f, fold.Share, 3);
    }

    [Fact]
    public void Share_AfterFolding_IsFolded()
    {
        var fold = new TabFold();
        fold.Fold();

        fold.Advance(TabFold.Duration);

        Assert.Equal(1, fold.Share);
    }

    [Fact]
    public void Share_HalfwayThroughUnfolding_IsEasedBackPastHalf()
    {
        var fold = FoldedFold();
        fold.Unfold();

        fold.Advance(TabFold.Duration / 2);

        Assert.Equal(0.125f, fold.Share, 3);
    }

    [Fact]
    public void Share_AfterUnfolding_IsUnfolded()
    {
        var fold = FoldedFold();
        fold.Unfold();

        fold.Advance(TabFold.Duration);

        Assert.Equal(0, fold.Share);
    }

    // Turning back mid-way starts from where the tabs are, then heads back.
    [Fact]
    public void Unfold_MidFold_KeepsTheShareItHad()
    {
        var fold = new TabFold();
        fold.Fold();
        fold.Advance(TabFold.Duration / 4);
        var midFold = fold.Share;

        fold.Unfold();

        Assert.Equal(midFold, fold.Share, 4);
        fold.Advance(TabFold.Duration / 10);
        Assert.True(fold.Share < midFold, $"The share went from {midFold} to {fold.Share} after turning back.");
    }

    // Cancel then Confirm quickly: the tabs turn back from where they are, then fold again.
    [Fact]
    public void Fold_MidUnfold_KeepsTheShareItHad()
    {
        var fold = FoldedFold();
        fold.Unfold();
        fold.Advance(TabFold.Duration / 4);
        var midUnfold = fold.Share;

        fold.Fold();

        Assert.Equal(midUnfold, fold.Share, 4);
        fold.Advance(TabFold.Duration / 10);
        Assert.True(fold.Share > midUnfold, $"The share went from {midUnfold} to {fold.Share} after turning back.");
    }

    [Fact]
    public void Fold_WhileAlreadyFolding_KeepsItsProgress()
    {
        var fold = new TabFold();
        fold.Fold();
        fold.Advance(TabFold.Duration / 2);

        fold.Fold();

        Assert.Equal(0.875f, fold.Share, 3);
    }

    [Fact]
    public void HiddenWidth_Folded_LeavesTheRailOnScreen()
    {
        var fold = FoldedFold();

        var shown = NavTabShape.Width - fold.HiddenWidth;

        Assert.Equal(TabFold.RailWidth, shown);
    }

    private static TabFold FoldedFold()
    {
        var fold = new TabFold();
        fold.Fold();
        fold.Advance(TabFold.Duration);
        return fold;
    }
}
