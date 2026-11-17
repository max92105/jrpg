using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Tests.GifImport;

// Frames scaled to a height (decision 637): nearest neighbor for a whole factor, an alpha-weighted average otherwise.
public sealed class FrameScalerTests
{
    private const uint Black = 0xFF000000;
    private const uint White = 0xFFFFFFFF;
    private const uint Red = 0xFFFF0000;

    [Fact]
    public void Scale_DoubleHeight_RepeatsEachPixelTwice()
    {
        uint[] checker = [Black, White, White, Black];

        var scaled = Assert.Single(FrameScaler.Scale([checker], 2, 2, 4));

        Assert.Equal(
            [Black, Black, White, White, Black, Black, White, White, White, White, Black, Black, White, White, Black, Black],
            scaled);
    }

    [Fact]
    public void Scale_HalfHeight_TakesEveryOtherPixel()
    {
        uint[] frame = [Black, White, Black, White, Red, Red, Red, Red, White, Black, White, Black, Red, Red, Red, Red];

        var scaled = Assert.Single(FrameScaler.Scale([frame], 4, 4, 2));

        Assert.Equal([Black, Black, White, White], scaled);
    }

    // A whole factor down, which Scale takes nearest neighbor: a battle background is averaged (decision 904).
    [Fact]
    public void ScaleAveraged_HalfHeight_IsTheGreyBetween()
    {
        uint[] checker = [Black, White, White, Black];

        var scaled = Assert.Single(FrameScaler.ScaleAveraged([checker], 2, 2, 1));

        Assert.Equal([0xFF808080], scaled);
    }

    // Black and white columns scaled to three quarters: the middle pixel covers two thirds of a white column and two
    // thirds of a black one, so it is the grey between.
    [Fact]
    public void Scale_ThreeQuarters_AveragesCoveredPixels()
    {
        uint[] columns = [.. Enumerable.Range(0, 16).Select(index => index % 2 == 0 ? Black : White)];

        var scaled = Assert.Single(FrameScaler.Scale([columns], 4, 4, 3));

        var red = (scaled[4] >> 16) & 0xFF;
        Assert.Equal((9, 0xFFu), (scaled.Length, scaled[4] >> 24));
        Assert.InRange(red, 126u, 129u);
    }

    // Opaque red in the left half, transparent in the right: the middle pixel stays red, at about half alpha.
    [Fact]
    public void Scale_AverageNextToTransparent_KeepsTheColor()
    {
        uint[] halfRed = [.. Enumerable.Range(0, 16).Select(index => index % 4 < 2 ? Red : 0u)];

        var middle = Assert.Single(FrameScaler.Scale([halfRed], 4, 4, 3))[4];

        Assert.Equal(0xFF0000u, middle & 0xFFFFFF);
        Assert.InRange(middle >> 24, 126u, 129u);
    }

    // 53 x 100 to 192 high: the step back from the target lands a hair past the source's edge in floating point.
    [Fact]
    public void Scale_UnevenStep_StaysInsideTheSource()
    {
        uint[] red = [.. Enumerable.Repeat(Red, 53 * 100)];

        var scaled = Assert.Single(FrameScaler.Scale([red], 53, 100, 192));

        Assert.Equal((102 * 192, Red), (scaled.Length, scaled[^1]));
    }

    [Fact]
    public void WidthAt_KeepsTheAspect()
    {
        Assert.Equal(150, FrameScaler.WidthAt(100, 128, 192));
    }

    [Fact]
    public void Scale_SameHeight_IsUnchanged()
    {
        IReadOnlyList<uint[]> frames = [[Black, White]];

        Assert.Same(frames, FrameScaler.Scale(frames, 2, 1, 1));
    }

    [Theory]
    [InlineData(64, 128)]
    [InlineData(256, 128)]
    public void IsWholeFactor_TwoAndHalf_AreWhole(int height, int targetHeight)
    {
        Assert.True(FrameScaler.IsWholeFactor(height, targetHeight));
    }

    [Fact]
    public void IsWholeFactor_ThreeQuarters_IsNot()
    {
        Assert.False(FrameScaler.IsWholeFactor(128, 96));
    }
}
