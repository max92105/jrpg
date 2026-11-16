using Jrpg.Editor.Battle;
using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Tests.Battle;

// The GIF import's dialog as typed (decisions 637, 638, 987): a height from 16 to 1024 px, its note, the background
// box, and the pace typed only for one battle sprite GIF; the GIFs are built in memory, 64 px high at 120 ms.
public sealed class GifImportOptionsViewModelTests
{
    [Fact]
    public void New_Default_IsHundredTwentyEightAndKeepsTheBackground()
    {
        var options = OneBattleGif();

        Assert.Equal(("128", false), (options.HeightText, options.RemovesBackground));
    }

    [Theory]
    [InlineData("15")]
    [InlineData("1025")]
    [InlineData("tall")]
    public void Problem_HeightOutsideTheRange_NamesTheRange(string height)
    {
        var options = OneBattleGif();

        options.HeightText = height;

        Assert.Equal("Battle height must be a whole number from 16 to 1024.", options.Problem);
    }

    [Theory]
    [InlineData("16")]
    [InlineData("1024")]
    public void Problem_HeightAtABound_IsNull(string height)
    {
        var options = OneBattleGif();

        options.HeightText = height;

        Assert.Null(options.Problem);
    }

    [Fact]
    public void FactorNote_DoubleTheFrame_SaysCrisp()
    {
        Assert.Equal("2x the GIF's 64 px: crisp", OneBattleGif().FactorNote);
    }

    [Fact]
    public void FactorNote_OddFactor_SaysSoftened()
    {
        var options = GifImportOptionsViewModel.ForBattleSprites(GifOf(height: 80), 1);

        Assert.Equal("1.6x the GIF's 80 px: softened", options.FactorNote);
    }

    [Fact]
    public void FactorNote_GifUnread_IsEmpty()
    {
        Assert.Equal("", GifImportOptionsViewModel.ForBattleSprites(null, 1).FactorNote);
    }

    [Fact]
    public void FactorNote_HeightTyped_IsRaisedAndReadsTheNewFactor()
    {
        var options = OneBattleGif();
        var notified = NoticesOf(options);

        options.HeightText = "100";

        Assert.Contains(nameof(GifImportOptionsViewModel.FactorNote), notified);
        Assert.Equal("1.56x the GIF's 64 px: softened", options.FactorNote);
    }

    [Fact]
    public void ToOptions_Typed_CarriesEachBox()
    {
        var options = OneBattleGif();
        options.HeightText = "192";
        options.RemovesBackground = true;

        Assert.Equal(new GifImportOptions(192, true) { FrameMilliseconds = 120 }, options.ToOptions());
    }

    [Fact]
    public void ToOptions_WithAProblem_Throws()
    {
        var options = OneBattleGif();
        options.HeightText = "15";

        Assert.Throws<InvalidOperationException>(() => options.ToOptions());
    }

    [Fact]
    public void PropertyChanged_CanApply_RaisedWhenHeightTyped()
    {
        var options = OneBattleGif();
        var notified = NoticesOf(options);

        options.HeightText = "15";

        Assert.Contains(nameof(GifImportOptionsViewModel.CanApply), notified);
        Assert.False(options.CanApply);
    }

    [Fact]
    public void FrameMillisecondsText_OneBattleGif_FilledWithItsFrameTime()
    {
        var options = OneBattleGif();

        Assert.Equal(("120", 120), (options.FrameMillisecondsText, options.FrameMilliseconds));
        Assert.Equal((true, ""), (options.IsPaceTyped, options.PaceNote));
    }

    [Fact]
    public void FrameMillisecondsText_GifUnread_IsHundred()
    {
        Assert.Equal("100", GifImportOptionsViewModel.ForBattleSprites(null, 1).FrameMillisecondsText);
    }

    [Fact]
    public void ToOptions_OneBattleGifTypedPace_Carried()
    {
        var options = OneBattleGif();
        options.FrameMillisecondsText = "150";

        Assert.Equal(150, options.ToOptions().FrameMilliseconds);
    }

    [Fact]
    public void ToOptions_SeveralGifs_CarriesNoPace()
    {
        var options = GifImportOptionsViewModel.ForBattleSprites(GifOf(height: 64), 2);

        Assert.Null(options.ToOptions().FrameMilliseconds);
        Assert.Equal((false, "each keeps its own pace"), (options.IsPaceTyped, options.PaceNote));
    }

    [Fact]
    public void ToOptions_EnemyEffect_CarriesNoPaceAndSaysSo()
    {
        var options = GifImportOptionsViewModel.ForEnemyEffects(GifOf(height: 64), 1);

        Assert.Null(options.ToOptions().FrameMilliseconds);
        Assert.Equal((false, "set its pace in Effect strips"), (options.IsPaceTyped, options.PaceNote));
    }

    [Fact]
    public void PaceNote_SeveralEnemyEffects_SaysTheirPace()
    {
        var options = GifImportOptionsViewModel.ForEnemyEffects(GifOf(height: 64), 2);

        Assert.Equal("set their pace in Effect strips", options.PaceNote);
    }

    // The greyed box plays the first GIF at its own pace, as each GIF keeps its own.
    [Fact]
    public void FrameMilliseconds_SeveralGifs_TheFirstOnesPace()
    {
        Assert.Equal(120, GifImportOptionsViewModel.ForBattleSprites(GifOf(height: 64), 2).FrameMilliseconds);
    }

    [Fact]
    public void Problem_PaceZero_SaysSo()
    {
        var options = OneBattleGif();

        options.FrameMillisecondsText = "0";

        Assert.Equal(("Frame time must be a whole number from 1.", null), (options.Problem, options.FrameMilliseconds));
    }

    [Fact]
    public void Problem_PaceOne_IsNull()
    {
        var options = OneBattleGif();

        options.FrameMillisecondsText = "1";

        Assert.Null(options.Problem);
    }

    // A greyed box isn't the import's, so it can't hold OK back.
    [Fact]
    public void Problem_PaceGreyed_IgnoresTheBox()
    {
        var options = GifImportOptionsViewModel.ForEnemyEffects(GifOf(height: 64), 1);

        options.FrameMillisecondsText = "0";

        Assert.Null(options.Problem);
    }

    [Fact]
    public void FrameMilliseconds_PaceTyped_RaisedWithProblemAndCanApply()
    {
        var options = OneBattleGif();
        var notified = NoticesOf(options);

        options.FrameMillisecondsText = "200";

        Assert.Equal(200, options.FrameMilliseconds);
        Assert.Contains(nameof(GifImportOptionsViewModel.FrameMilliseconds), notified);
        Assert.Contains(nameof(GifImportOptionsViewModel.Problem), notified);
        Assert.Contains(nameof(GifImportOptionsViewModel.CanApply), notified);
    }

    private static GifImportOptionsViewModel OneBattleGif() => GifImportOptionsViewModel.ForBattleSprites(GifOf(height: 64), 1);

    private static ComposedGif GifOf(int height) => new([], 1, height, 120);

    private static List<string?> NoticesOf(GifImportOptionsViewModel options)
    {
        var notified = new List<string?>();
        options.PropertyChanged += (_, e) => notified.Add(e.PropertyName);
        return notified;
    }
}
