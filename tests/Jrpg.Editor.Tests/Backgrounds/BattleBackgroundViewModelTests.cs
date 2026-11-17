using Jrpg.Core.Sprites;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Backgrounds;
using static Jrpg.Editor.Tests.Backgrounds.BackgroundFixtures;

namespace Jrpg.Editor.Tests.Backgrounds;

// One background image as the Battle backgrounds page shows it (decisions 901, 902), still or animated by its record.
public sealed class BattleBackgroundViewModelTests
{
    private const string Still = "Forest.png";

    private static readonly (int Width, int Height) _oneColumnSheet = (960, 1080);

    [Fact]
    public void From_NoRecord_IsStillWithItsSize()
    {
        var background = BattleBackgroundViewModel.From(Still, null, PulseSheet);

        Assert.Equal(
            (false, false, "Still, 1920 x 1080 px"), (background.IsAnimated, background.IsImageMissing, background.KindLine));
    }

    [Fact]
    public void From_Record_IsAnimatedWithItsTime()
    {
        var background = Animated();

        Assert.Equal(
            (true, "200", "Animated: 4 frames of 960 x 540 px in a 2 x 2 grid"),
            (background.IsAnimated, background.FrameMillisecondsText, background.KindLine));
    }

    [Fact]
    public void From_SizeNull_IsImageMissing()
    {
        var background = BattleBackgroundViewModel.From(Pulse.File, Pulse, null);

        Assert.Equal((true, "Image missing"), (background.IsImageMissing, background.KindLine));
    }

    [Fact]
    public void From_NoRecord_HasNoFrameTimeNorFrameSize()
    {
        var background = BattleBackgroundViewModel.From(Still, null, PulseSheet);

        Assert.Equal(("", null), (background.FrameMillisecondsText, background.FrameSize));
    }

    [Fact]
    public void FrameSize_Record_IsItsFrameSize() => Assert.Equal((960, 540), Animated().FrameSize);

    [Fact]
    public void PreviewCells_TwoByTwoGrid_AreTheCellsInRowOrder()
    {
        SpriteCell[] expected = [new(0, 0), new(1, 0), new(0, 1), new(1, 1)];

        Assert.Equal(expected, Animated().PreviewCells);
    }

    [Fact]
    public void PreviewCells_ThreeColumnSheet_FillTheFirstRowFirst()
    {
        SpriteCell[] expected = [new(0, 0), new(1, 0), new(2, 0), new(0, 1)];

        var background = BattleBackgroundViewModel.From(Pulse.File, Pulse, (2880, 1080));

        Assert.Equal(expected, background.PreviewCells);
    }

    [Fact]
    public void KindLine_ThreeColumnSheet_IsAThreeByTwoGrid()
    {
        var background = BattleBackgroundViewModel.From(Pulse.File, Pulse, (2880, 1080));

        Assert.Equal("Animated: 4 frames of 960 x 540 px in a 3 x 2 grid", background.KindLine);
    }

    [Fact]
    public void PreviewCells_Still_AreEmpty() =>
        Assert.Empty(BattleBackgroundViewModel.From(Still, null, PulseSheet).PreviewCells);

    [Fact]
    public void PreviewCells_ImageNarrowerThanAFrame_AreEmpty()
    {
        var background = BattleBackgroundViewModel.From(Pulse.File, Pulse, (480, 2160));

        Assert.Empty(background.PreviewCells);
    }

    [Fact]
    public void PreviewCells_FrameWidthZero_AreEmpty()
    {
        var background = BattleBackgroundViewModel.From(Pulse.File, Pulse with { FrameWidth = 0 }, PulseSheet);

        Assert.Empty(background.PreviewCells);
    }

    [Fact]
    public void KindLine_ImageTooSmallForTheGrid_IsTheImagesSize()
    {
        var background = BattleBackgroundViewModel.From(Pulse.File, Pulse, _oneColumnSheet);

        Assert.Equal("Animated, 960 x 1080 px", background.KindLine);
    }

    [Fact]
    public void Problem_TimeZero_NamesTheFrameTime()
    {
        var background = Animated();

        background.FrameMillisecondsText = "0";

        Assert.Equal("Frame time must be a whole number of milliseconds from 1.", background.Problem);
    }

    [Fact]
    public void Problem_TimeOne_IsNull()
    {
        var background = Animated();

        background.FrameMillisecondsText = "1";

        Assert.Null(background.Problem);
    }

    [Fact]
    public void Problem_TimeNotANumber_NamesTheFrameTime()
    {
        var background = Animated();

        background.FrameMillisecondsText = "fast";

        Assert.Equal("Frame time must be a whole number of milliseconds from 1.", background.Problem);
    }

    [Fact]
    public void Problem_ImageTooSmallForTheGrid_IsDatasGridLine()
    {
        var background = BattleBackgroundViewModel.From(Pulse.File, Pulse, _oneColumnSheet);

        Assert.Equal(ArtReferenceCheck.GridProblemOf(Pulse, _oneColumnSheet), background.Problem);
    }

    [Fact]
    public void Problem_FrameWidthZero_IsDatasRecordLine()
    {
        var record = Pulse with { FrameWidth = 0 };

        var background = BattleBackgroundViewModel.From(Pulse.File, record, PulseSheet);

        Assert.Equal(CatalogProblems.OfAnimatedBackgrounds([record]), background.Problem);
    }

    [Fact]
    public void Problem_AnimatedImageMissing_IsNull() =>
        Assert.Null(BattleBackgroundViewModel.From(Pulse.File, Pulse, null).Problem);

    [Fact]
    public void Problem_Still_IsNull() => Assert.Null(BattleBackgroundViewModel.From(Still, null, PulseSheet).Problem);

    [Fact]
    public void FrameMillisecondsText_Changed_RaisesProblem()
    {
        var background = Animated();
        List<string?> raised = [];
        background.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        background.FrameMillisecondsText = "0";

        Assert.Contains(nameof(BattleBackgroundViewModel.Problem), raised);
    }

    [Fact]
    public void ToRecord_TypedTime_IsTheRecordWithThatTime()
    {
        var background = Animated();

        background.FrameMillisecondsText = "350";

        Assert.Equal(Pulse with { Animation = Pulse.Animation with { FrameMilliseconds = 350 } }, background.ToRecord());
    }

    [Fact]
    public void ToRecord_Untouched_EqualsTheRecord() => Assert.Equal(Pulse, Animated().ToRecord());

    [Fact]
    public void ToRecord_Still_IsNull() => Assert.Null(BattleBackgroundViewModel.From(Still, null, PulseSheet).ToRecord());

    [Fact]
    public void ToRecord_WithAProblem_Throws()
    {
        var background = Animated();
        background.FrameMillisecondsText = "0";

        Assert.Throws<InvalidOperationException>(background.ToRecord);
    }

    private static BattleBackgroundViewModel Animated() => BattleBackgroundViewModel.From(Pulse.File, Pulse, PulseSheet);
}
