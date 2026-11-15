using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

// The actor's double blink (decision 648): full from 0.10 s for 33 ms, then from 0.20 s for 100 ms, the last 30 fading.
public sealed class ActorFlashTests
{
    [Fact]
    public void StrengthAt_BeforeTheFirstBlink_IsZero()
    {
        Assert.Equal(0, ActorFlash.StrengthAt(TimeSpan.FromSeconds(0.09)));
    }

    [Fact]
    public void StrengthAt_FirstBlink_IsFull()
    {
        Assert.Equal(1, ActorFlash.StrengthAt(TimeSpan.FromSeconds(0.10)));
    }

    [Fact]
    public void StrengthAt_FirstBlinkOver_IsZero()
    {
        Assert.Equal(0, ActorFlash.StrengthAt(TimeSpan.FromSeconds(0.134)));
    }

    [Theory]
    [InlineData(0.20)]
    [InlineData(0.26)]
    public void StrengthAt_SecondBlink_IsFull(double seconds)
    {
        Assert.Equal(1, ActorFlash.StrengthAt(TimeSpan.FromSeconds(seconds)));
    }

    [Fact]
    public void StrengthAt_SecondBlinksEnd_IsFading()
    {
        Assert.Equal(0.5f, ActorFlash.StrengthAt(TimeSpan.FromSeconds(0.285)), 0.001f);
    }

    [Fact]
    public void StrengthAt_AfterTheBlinks_IsZero()
    {
        Assert.Equal(0, ActorFlash.StrengthAt(TimeSpan.FromSeconds(0.30)));
    }
}
