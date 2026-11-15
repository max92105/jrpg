using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// The GIF import's dialog as typed (decisions 637, 638): a height from 16 to 1024 px, its note, the background box.
public sealed class GifImportOptionsViewModelTests
{
    [Fact]
    public void New_Default_IsHundredTwentyEightAndKeepsTheBackground()
    {
        var options = new GifImportOptionsViewModel(64);

        Assert.Equal(("128", false), (options.HeightText, options.RemovesBackground));
    }

    [Theory]
    [InlineData("15")]
    [InlineData("1025")]
    [InlineData("tall")]
    public void Problem_HeightOutsideTheRange_NamesTheRange(string height)
    {
        var options = new GifImportOptionsViewModel(64) { HeightText = height };

        Assert.Equal("Battle height must be a whole number from 16 to 1024.", options.Problem);
    }

    [Theory]
    [InlineData("16")]
    [InlineData("1024")]
    public void Problem_HeightAtABound_IsNull(string height)
    {
        Assert.Null(new GifImportOptionsViewModel(64) { HeightText = height }.Problem);
    }

    [Fact]
    public void FactorNote_DoubleTheFrame_SaysCrisp()
    {
        Assert.Equal("2x the GIF's 64 px: crisp", new GifImportOptionsViewModel(64).FactorNote);
    }

    [Fact]
    public void FactorNote_OddFactor_SaysSoftened()
    {
        Assert.Equal("1.6x the GIF's 80 px: softened", new GifImportOptionsViewModel(80).FactorNote);
    }

    [Fact]
    public void FactorNote_NoFrameHeight_IsEmpty()
    {
        Assert.Equal("", new GifImportOptionsViewModel(null).FactorNote);
    }

    [Fact]
    public void FactorNote_HeightTyped_IsRaisedAndReadsTheNewFactor()
    {
        var options = new GifImportOptionsViewModel(64);
        var notified = new List<string?>();
        options.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        options.HeightText = "100";

        Assert.Contains(nameof(GifImportOptionsViewModel.FactorNote), notified);
        Assert.Equal("1.56x the GIF's 64 px: softened", options.FactorNote);
    }

    [Fact]
    public void ToOptions_Typed_CarriesBoth()
    {
        var options = new GifImportOptionsViewModel(64) { HeightText = "192", RemovesBackground = true };

        Assert.Equal(new(192, true), options.ToOptions());
    }

    [Fact]
    public void ToOptions_WithAProblem_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new GifImportOptionsViewModel(64) { HeightText = "15" }.ToOptions());
    }

    [Fact]
    public void PropertyChanged_CanApply_RaisedWhenHeightTyped()
    {
        var options = new GifImportOptionsViewModel(64);
        var notified = new List<string?>();
        options.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        options.HeightText = "15";

        Assert.Contains(nameof(GifImportOptionsViewModel.CanApply), notified);
        Assert.False(options.CanApply);
    }
}
