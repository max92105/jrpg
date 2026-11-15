using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

// How far the impact's bright parts split (decision 661): 6 px at the impact, closing to none by 0.167 s, and the whole
// pixels its copies are drawn apart.
public sealed class ColorFringeTests
{
    [Fact]
    public void ShiftAt_TheImpact_IsTheWholeShift()
    {
        Assert.Equal(6, ColorFringe.ShiftAt(TimeSpan.Zero));
    }

    [Fact]
    public void ShiftAt_HalfTheTime_IsAnEighth()
    {
        // (1 - 0.5)³ of the whole: it closes fast, then settles.
        Assert.Equal(0.75f, ColorFringe.ShiftAt(TimeSpan.FromSeconds(0.0835)), 0.0001f);
    }

    [Fact]
    public void ShiftAt_JustBeforeItsEnd_IsAboveZero()
    {
        Assert.True(ColorFringe.ShiftAt(TimeSpan.FromSeconds(0.16)) > 0);
    }

    [Fact]
    public void ShiftAt_ItsEnd_IsZero()
    {
        Assert.Equal(0, ColorFringe.ShiftAt(TimeSpan.FromSeconds(0.167)));
    }

    [Fact]
    public void ShiftAt_PastItsEnd_IsZero()
    {
        Assert.Equal(0, ColorFringe.ShiftAt(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void ShiftAt_BeforeTheImpact_IsZero()
    {
        Assert.Equal(0, ColorFringe.ShiftAt(TimeSpan.FromSeconds(-0.01)));
    }

    [Fact]
    public void PixelsAt_TheImpact_IsSix()
    {
        Assert.Equal(6, ColorFringe.PixelsAt(TimeSpan.Zero));
    }

    [Fact]
    public void PixelsAt_HalfTheTime_RoundsUpToOne()
    {
        Assert.Equal(1, ColorFringe.PixelsAt(TimeSpan.FromSeconds(0.0835)));
    }

    [Fact]
    public void PixelsAt_UnderHalfAPixel_IsNone()
    {
        // At 0.14 s the shift is about 0.03 px.
        Assert.Equal(0, ColorFringe.PixelsAt(TimeSpan.FromSeconds(0.14)));
    }
}
