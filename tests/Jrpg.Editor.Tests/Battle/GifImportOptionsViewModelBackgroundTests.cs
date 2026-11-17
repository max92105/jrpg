using Jrpg.Editor.Battle;
using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Tests.Battle;

// The GIF dialog's background kind (decisions 898, 903, 904): the heights offered, the one chosen first, the sheet
// line, the problem line and the preview's frames; the GIFs are built in memory at 120 ms, their frames empty where
// only their count matters.
public sealed class GifImportOptionsViewModelBackgroundTests
{
    private const uint Black = 0xFF000000;
    private const uint White = 0xFFFFFFFF;
    private const uint Grey = 0xFF808080;

    [Fact]
    public void HeightChoices_GifOf443_AreItsOwnThen360Down()
    {
        var options = GifImportOptionsViewModel.ForBackground(GifOf(443, 443, frameCount: 8));

        Assert.Equal(
            [(443, "443 px (the GIF's)"), (360, "360 px"), (270, "270 px"), (216, "216 px"), (180, "180 px")],
            options.HeightChoices.Select(choice => (choice.Height, choice.Label)));
    }

    [Fact]
    public void HeightChoices_GifTallerThan1080_StartAt1080()
    {
        var options = GifImportOptionsViewModel.ForBackground(GifOf(1200, 1200, frameCount: 2));

        Assert.Equal(
            [(1080, "1080 px"), (540, "540 px"), (360, "360 px"), (270, "270 px"), (216, "216 px"), (180, "180 px")],
            options.HeightChoices.Select(choice => (choice.Height, choice.Label)));
    }

    [Fact]
    public void HeightChoices_GifOf1080_OffersItOnce()
    {
        var options = GifImportOptionsViewModel.ForBackground(GifOf(1920, 1080, frameCount: 2));

        Assert.Equal([1080, 540, 360, 270, 216, 180], options.HeightChoices.Select(choice => choice.Height));
    }

    [Fact]
    public void HeightChoices_GifBelow180_IsItsOwnAlone()
    {
        var options = GifImportOptionsViewModel.ForBackground(GifOf(100, 100, frameCount: 2));

        Assert.Equal([(100, "100 px (the GIF's)")], options.HeightChoices.Select(choice => (choice.Height, choice.Label)));
    }

    // The import then reads the file again and names it among its refusals.
    [Fact]
    public void HeightChoices_GifUnread_AreTheDivisorsTheTallestChosen()
    {
        var options = GifImportOptionsViewModel.ForBackground(null);

        Assert.Equal([1080, 540, 360, 270, 216, 180], options.HeightChoices.Select(choice => choice.Height));
        Assert.Equal((1080, "", null), (options.SelectedHeight, options.SheetLine, options.Problem));
    }

    [Fact]
    public void SelectedHeight_GifOf443WithEightFrames_IsItsOwn()
    {
        Assert.Equal(443, GifImportOptionsViewModel.ForBackground(GifOf(443, 443, frameCount: 8)).SelectedHeight);
    }

    // 1920 px frames at 1080: two columns of three rows hold 6, so 6 is the bound and 7 is past it.
    [Theory]
    [InlineData(6, 1080)]
    [InlineData(7, 540)]
    [InlineData(24, 540)]
    public void SelectedHeight_FullScreenGif_IsTheTallestHoldingEveryFrame(int frameCount, int expected)
    {
        var options = GifImportOptionsViewModel.ForBackground(GifOf(1920, 1080, frameCount));

        Assert.Equal(expected, options.SelectedHeight);
    }

    // At 180 px a 320 px frame fits 12 columns of 22 rows: 264 frames, so 300 fit no height.
    [Fact]
    public void SelectedHeight_NoHeightHoldsEveryFrame_IsItsOwnAndTheProblemSaysSo()
    {
        var options = GifImportOptionsViewModel.ForBackground(GifOf(1920, 1080, frameCount: 300));

        Assert.Equal(1080, options.SelectedHeight);
        Assert.Equal("300 frames don't fit a 4096 px sheet at 1080 px (6 at most): choose a smaller height.", options.Problem);
    }

    [Fact]
    public void SheetLine_24FramesOf480By270_NamesSheetGridAndMemory()
    {
        var options = GifImportOptionsViewModel.ForBackground(GifOf(480, 270, frameCount: 24));

        Assert.Equal(
            "24 frames of 480 x 270 px: a 3840 x 810 sheet (8 x 3), 12.4 MB of video memory", options.SheetLine);
    }

    [Fact]
    public void SheetLine_FramesDontFit_IsEmpty()
    {
        var options = GifImportOptionsViewModel.ForBackground(GifOf(1920, 1080, frameCount: 24));

        options.SelectedHeight = 1080;

        Assert.Equal("", options.SheetLine);
    }

    [Fact]
    public void SheetLine_BattleSpriteKind_IsEmpty()
    {
        Assert.Equal("", GifImportOptionsViewModel.ForBattleSprites(GifOf(480, 270, frameCount: 24), 1).SheetLine);
    }

    [Fact]
    public void Problem_TooManyFramesAtTheHeight_NamesTheMost()
    {
        var options = GifImportOptionsViewModel.ForBackground(GifOf(1920, 1080, frameCount: 24));

        options.SelectedHeight = 1080;

        Assert.Equal("24 frames don't fit a 4096 px sheet at 1080 px (6 at most): choose a smaller height.", options.Problem);
        Assert.False(options.CanApply);
    }

    // 8000 x 300 is 4800 px wide at 180 px, the smallest height offered: no height makes a sheet.
    [Fact]
    public void Problem_FramesTooWideAtEveryHeight_SaysNoHeightFits()
    {
        var options = GifImportOptionsViewModel.ForBackground(GifOf(8000, 300, frameCount: 2));

        Assert.Equal(
            "Its frames are wider than 4096 px at every height offered (4800 px at 180 px): it can't be a battle background.",
            options.Problem);
    }

    // 5000 x 300 is 3000 px wide at 180 px, so a smaller height fits even though 300 px holds none.
    [Fact]
    public void Problem_FramesTooWideAtTheChosenHeightOnly_AsksForASmallerHeight()
    {
        var options = GifImportOptionsViewModel.ForBackground(GifOf(5000, 300, frameCount: 2));

        options.SelectedHeight = 300;

        Assert.Equal("2 frames don't fit a 4096 px sheet at 300 px (0 at most): choose a smaller height.", options.Problem);
    }

    [Fact]
    public void Problem_BackgroundPaceZero_NamesTheFrameTime()
    {
        var options = GifImportOptionsViewModel.ForBackground(GifOf(443, 443, frameCount: 8));

        options.FrameMillisecondsText = "0";

        Assert.Equal("Frame time must be a whole number from 1.", options.Problem);
    }

    // The typed height box is the other kinds'; it is hidden here and never read.
    [Fact]
    public void Problem_BackgroundHeightBoxUnreadable_IsNull()
    {
        var options = GifImportOptionsViewModel.ForBackground(GifOf(443, 443, frameCount: 8));

        options.HeightText = "tall";

        Assert.Null(options.Problem);
    }

    [Fact]
    public void SelectedHeight_Changed_RaisesSheetLineProblemAndPreview()
    {
        var options = GifImportOptionsViewModel.ForBackground(GifOf(443, 443, frameCount: 8));
        var notified = new List<string?>();
        options.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        options.SelectedHeight = 360;

        Assert.Equal(
            [
                nameof(GifImportOptionsViewModel.SelectedHeight),
                nameof(GifImportOptionsViewModel.Problem),
                nameof(GifImportOptionsViewModel.CanApply),
                nameof(GifImportOptionsViewModel.SheetLine),
                nameof(GifImportOptionsViewModel.PreviewAtChosenHeight),
            ],
            notified);
    }

    // The pace typed is 150, told from the GIF's 120; the height chosen is 360, below the GIF's 443.
    [Fact]
    public void ToOptions_Background_NeverClearsAndCarriesTheChosenHeightAndPace()
    {
        var options = GifImportOptionsViewModel.ForBackground(GifOf(443, 443, frameCount: 8));
        options.SelectedHeight = 360;
        options.FrameMillisecondsText = "150";

        var chosen = options.ToOptions();

        Assert.Equal(new GifImportOptions(360, RemovesBackground: false) { FrameMilliseconds = 150 }, chosen);
    }

    // Rows of black and white, halved: every pixel is the grey average, which nearest neighbor would never give.
    [Fact]
    public void PreviewAtChosenHeight_HalfTheHeight_AveragesTheFrames()
    {
        uint[] rows = [.. Enumerable.Range(0, 2 * 360).Select(index => index / 2 % 2 == 0 ? Black : White)];
        var options = GifImportOptionsViewModel.ForBackground(new ComposedGif([rows, rows], 2, 360, 120));

        options.SelectedHeight = 180;

        var preview = options.PreviewAtChosenHeight!;
        Assert.Equal((1, 180, 2), (preview.Width, preview.Height, preview.Frames.Count));
        Assert.Equal(Enumerable.Repeat(Grey, 180), preview.Frames[0]);
    }

    [Fact]
    public void PreviewAtChosenHeight_TheGifsOwn_IsTheGifAsRead()
    {
        var gif = GifOf(443, 443, frameCount: 8);

        var options = GifImportOptionsViewModel.ForBackground(gif);

        Assert.Same(gif, options.PreviewAtChosenHeight);
    }

    [Fact]
    public void PreviewAtChosenHeight_HeightChosenAgain_IsWorkedOutOnce()
    {
        uint[] frame = [.. Enumerable.Repeat(Black, 2 * 360)];
        var options = GifImportOptionsViewModel.ForBackground(new ComposedGif([frame, frame], 2, 360, 120));
        options.SelectedHeight = 180;
        var first = options.PreviewAtChosenHeight;
        options.SelectedHeight = 360;

        options.SelectedHeight = 180;

        Assert.Same(first, options.PreviewAtChosenHeight);
    }

    [Fact]
    public void PreviewAtChosenHeight_BattleSpriteKind_IsNull()
    {
        Assert.Null(GifImportOptionsViewModel.ForBattleSprites(GifOf(443, 443, frameCount: 8), 1).PreviewAtChosenHeight);
    }

    private static ComposedGif GifOf(int width, int height, int frameCount) =>
        new([.. Enumerable.Repeat(Array.Empty<uint>(), frameCount)], width, height, 120);
}
