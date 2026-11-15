using Jrpg.Core.Cinematics;

namespace Jrpg.Core.Tests.Cinematics;

// A still's opacity over its time (decision 629): in from black over the fade, holding, out to black, the fade at most
// half the still; on a 4 s still unless the test says otherwise.
public sealed class StillFadeTests
{
    private static readonly TimeSpan _fourSeconds = TimeSpan.FromSeconds(4);

    [Fact]
    public void OpacityAt_Start_IsZero()
    {
        Assert.Equal(0f, StillFade.OpacityAt(TimeSpan.Zero, _fourSeconds));
    }

    [Fact]
    public void OpacityAt_AfterTheFade_IsOne()
    {
        Assert.Equal(1f, StillFade.OpacityAt(StillFade.FadeTime, _fourSeconds));
    }

    [Fact]
    public void OpacityAt_HalfwayThroughTheFadeOut_IsHalf()
    {
        Assert.Equal(0.5f, StillFade.OpacityAt(_fourSeconds - (StillFade.FadeTime / 2), _fourSeconds), precision: 3);
    }

    [Fact]
    public void OpacityAt_StillShorterThanTwoFades_PeaksAtHalfway()
    {
        Assert.Equal(1f, StillFade.OpacityAt(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(1)), precision: 3);
    }

    [Fact]
    public void OpacityAt_End_IsZero()
    {
        Assert.Equal(0f, StillFade.OpacityAt(_fourSeconds, _fourSeconds));
    }

    [Fact]
    public void OpacityAt_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StillFade.OpacityAt(TimeSpan.FromTicks(-1), _fourSeconds));
    }
}
