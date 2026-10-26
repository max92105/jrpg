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

    // The labels are gone by the fold's middle, so no half-word shows at the screen's edge.
    [Fact]
    public void LabelOpacity_HalfwayThroughFolding_IsZero()
    {
        var fold = new TabFold();
        fold.Fold();

        fold.Advance(TabFold.Duration / 2);

        Assert.Equal(0, fold.LabelOpacity);
    }

    [Fact]
    public void LabelOpacity_QuarterIntoUnfolding_IsStillZero()
    {
        var fold = FoldedFold();
        fold.Unfold();

        fold.Advance(TabFold.Duration / 4);

        Assert.Equal(0, fold.LabelOpacity);
    }

    [Fact]
    public void LabelOpacity_AfterUnfolding_IsOne()
    {
        var fold = FoldedFold();
        fold.Unfold();

        fold.Advance(TabFold.Duration);

        Assert.Equal(1, fold.LabelOpacity);
    }

    // Frame by frame, as the menu runs: from the unfold's middle the labels come back at twice the tabs' pace, so they
    // are half back three quarters of the way through, and land with the tabs.
    [Fact]
    public void LabelOpacity_ThreeQuartersIntoUnfolding_IsHalfBack()
    {
        var fold = FoldedFold();
        fold.Unfold();

        for (var frame = 0; frame < 15; frame++)
        {
            fold.Advance(TabFold.Duration / 20);
        }

        Assert.InRange(fold.LabelOpacity, 0.4f, 0.6f);
    }

    // Folding again late in an unfold, the labels fade from where they had come back to.
    [Fact]
    public void Fold_LateInAnUnfold_KeepsTheLabelsFade()
    {
        var fold = FoldedFold();
        fold.Unfold();
        for (var frame = 0; frame < 16; frame++)
        {
            fold.Advance(TabFold.Duration / 20);
        }

        var returning = fold.LabelOpacity;
        Assert.InRange(returning, 0.1f, 0.9f);

        fold.Fold();
        fold.Advance(TabFold.Duration / 100);

        Assert.InRange(fold.LabelOpacity, returning - 0.1f, returning);
    }

    // Turning back early in a fold, the labels come back from where their fade was, never from nothing.
    [Fact]
    public void Unfold_EarlyInAFold_KeepsTheLabelsFade()
    {
        var fold = new TabFold();
        fold.Fold();
        fold.Advance(TabFold.Duration / 10);
        var fading = fold.LabelOpacity;
        Assert.InRange(fading, 0.1f, 0.9f);

        fold.Unfold();
        fold.Advance(TabFold.Duration / 100);

        Assert.True(fold.LabelOpacity >= fading, $"The labels went from {fading} to {fold.LabelOpacity} after turning back.");
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
